{ kubenixconfig, pkgs, tpvsel, ... }:
let
  kubenixScript = pkgs.writeShellScriptBin "kubenixScript" ''
    #!${pkgs.runtimeShell}
    set -euo pipefail

    FLAG_FILE="/var/lib/kubenixconfig/ran.flag"
    KUBECONFIG="/var/lib/rancher/k3s/server/cred/admin.kubeconfig"

    if [ -f "$FLAG_FILE" ]; then
      echo "Flag file '$FLAG_FILE' already exists. Nothing to do. ✅"
      exit 0
    fi

    echo "Waiting for k3s API to be ready..."
    for i in $(seq 1 60); do
      if ${pkgs.kubectl}/bin/kubectl --kubeconfig=$KUBECONFIG version --request-timeout=3s >/dev/null 2>&1; then
        echo "k3s API is ready. Proceeding."
        break
      fi
      echo "k3s not ready yet ($i/60)..."
      sleep 2
    done

    if ! ${pkgs.kubectl}/bin/kubectl --kubeconfig=$KUBECONFIG version --request-timeout=3s >/dev/null 2>&1; then
      echo "Timeout waiting for k3s API. Giving up."
      exit 1
    fi

    echo "Applying Kubernetes config..."
    cat ${kubenixconfig.defaultPackage.${pkgs.system}}/kube.json
    ${pkgs.kubectl}/bin/kubectl --kubeconfig=$KUBECONFIG apply -f ${kubenixconfig.defaultPackage.${pkgs.system}}/kube.json

    mkdir -p /var/lib/kubenixconfig
    echo "Kubenix config script ran at $(date)" > "$FLAG_FILE"
    echo "Initial run complete. Flag file created at '$FLAG_FILE'."
  '';
  
  ensureDefaultSaScript = pkgs.writeShellScriptBin "ensureDefaultSaScript" ''
    #!${pkgs.runtimeShell}
    set -euo pipefail

    KUBECTL=${pkgs.kubectl}/bin/kubectl
    CONFIG=/var/lib/rancher/k3s/server/cred/admin.kubeconfig

    echo "Waiting for K3s API to be ready..."
    # Wait until kubectl actually returns something sensible
    for i in {1..60}; do
      if $KUBECTL --kubeconfig=$CONFIG get nodes >/dev/null 2>&1; then
        break
      fi
      sleep 2
    done

    echo "Ensuring namespace and service account exist..."
    $KUBECTL --kubeconfig=$CONFIG get ns default >/dev/null 2>&1 || \
      $KUBECTL --kubeconfig=$CONFIG create namespace default

    $KUBECTL --kubeconfig=$CONFIG get serviceaccount default -n default >/dev/null 2>&1 || \
      $KUBECTL --kubeconfig=$CONFIG create serviceaccount default -n default
  '';
in
{
  systemd.services.ensure-default-sa = {
    description = "Ensure Kubernetes default service account exists";
    after = [ "k3s.service" ];
    requires = [ "k3s.service" ];
    wantedBy = [ "multi-user.target" ];
    serviceConfig = {
      Type = "oneshot";
      ExecStart = "${ensureDefaultSaScript}/bin/ensureDefaultSaScript";
      RemainAfterExit = true;
    };
  };

  systemd.services.kubenixconfig = {
    enable = true;
    description = "Run kubectl command and create a flag file once k3s is ready";

    # Proper dependencies
    after = [ "network-online.target" "k3s.service" "ensure-default-sa.service" ];
    requires = [ "k3s.service" "ensure-default-sa.service" ];
    wantedBy = [ "multi-user.target" ];

    serviceConfig = {
      Type = "oneshot";
      RemainAfterExit = true;
      ExecStart = "${kubenixScript}/bin/kubenixScript";
    };
  };

  environment.systemPackages = [
    pkgs.curl
    pkgs.gitMinimal
    pkgs.neofetch
    pkgs.htop
    pkgs.iproute2
    pkgs.kubectl
    kubenixconfig.defaultPackage.${pkgs.system}
    tpvsel.packages.${pkgs.system}.default
    # pkgs.docker
  ];

  networking.firewall = {
    enable = true;
    allowedTCPPorts = [ 80 443 1235 31890 31891 31892 31893 31894 31895 ];
    allowPing = true;
  };
}
