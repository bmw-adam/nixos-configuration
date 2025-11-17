{ kubenixconfig, pkgs, tpvsel, config, ... }:
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

  grafanaHelmValuesScript = pkgs.writeShellScriptBin "grafanaHelmValuesScript" ''
    echo "Initializing Grafana Helm values Secret creation script..."
    set -e
    echo "Starting Grafana Helm values Secret creation script..."
    
    # This is the path you specified
    SECRET_NAME="grafana-helm-values"
    # This MUST match the 'targetNamespace' in your HelmChart spec
    NAMESPACE="kube-system" 

    echo "Using template file at: ${config.sops.templates."grafana-cloud-metrics.yaml".path}"

    echo "Waiting for ${config.sops.templates."grafana-cloud-metrics.yaml".path}..."

    echo "Using template file at: ${config.sops.templates."grafana-cloud-metrics.yaml".path}"
    # Loop until the file exists
    while [ ! -f "${config.sops.templates."grafana-cloud-metrics.yaml".path}" ]; do
      sleep 2
    done
    echo "File found. Creating/updating secret $SECRET_NAME in $NAMESPACE..."

    # This command creates a secret 'from' that file.
    # We use 'create --dry-run' and pipe to 'apply' to make this
    # command idempotent: it will create *or* update the secret.
    ${pkgs.kubectl}/bin/kubectl --kubeconfig=/var/lib/rancher/k3s/server/cred/admin.kubeconfig create secret generic "$SECRET_NAME" \
      --namespace="$NAMESPACE" \
      --from-file=values.yaml="${config.sops.templates."grafana-cloud-metrics.yaml".path}" \
      --dry-run=client -o yaml | ${pkgs.kubectl}/bin/kubectl --kubeconfig=/var/lib/rancher/k3s/server/cred/admin.kubeconfig apply -f -

    echo "Secret $SECRET_NAME applied successfully."
  '';

  ybSecretGenerate = pkgs.writeShellScriptBin "ybSecretGenerate" ''
    #!${pkgs.runtimeShell}
    echo "Initializing ybSecretGenerate creation script..."
    set -e
    echo "Starting Grafana ybSecretGenerate creation script..."
    
    # This is the path you specified
    SECRET_NAME="yb-auth-secret"
    # This MUST match the 'targetNamespace' in your HelmChart spec
    NAMESPACE="default" 

    echo "Using password file at: ${config.sops.secrets.ysqlPassword.path}"

    echo "Waiting for ${config.sops.secrets.ysqlPassword.path}..."

    echo "Using template file at: ${config.sops.secrets.ysqlPassword.path}"
    # Loop until the file exists
    while [ ! -f "${config.sops.secrets.ysqlPassword.path}" ]; do
      sleep 2
    done
    echo "File found. Creating/updating secret $SECRET_NAME in $NAMESPACE..."

    # This command creates a secret 'from' that file.
    # We use 'create --dry-run' and pipe to 'apply' to make this
    # command idempotent: it will create *or* update the secret.
    ${pkgs.kubectl}/bin/kubectl --kubeconfig=/var/lib/rancher/k3s/server/cred/admin.kubeconfig create secret generic "$SECRET_NAME" \
      --namespace="$NAMESPACE" \
      --from-file=YSQL_PASSWORD="${config.sops.secrets.ysqlPassword.path}" \
      --dry-run=client -o yaml | ${pkgs.kubectl}/bin/kubectl --kubeconfig=/var/lib/rancher/k3s/server/cred/admin.kubeconfig apply -f -

    echo "Secret $SECRET_NAME applied successfully."

    # Wait until YSQL is accepting connections
    echo "Waiting for YSQL to accept connections..."
    MAX_ATTEMPTS=20
    SLEEP_INTERVAL=15
    attempt=1

    KUBECONFIG_PATH="/var/lib/rancher/k3s/server/cred/admin.kubeconfig"
    POD_NAME="yb-tserver-0"
    NAMESPACE="default"
    YSQL_HOST="yb-tserver-0.yb-tservers.default"
    YSQL_USER="yugabyte"
    YSQL_PORT=5433
    YSQL_PASSWORD="yugabyte"

    until kubectl --kubeconfig="$KUBECONFIG_PATH" exec -n "$NAMESPACE" -it "$POD_NAME" -- \
        env PGPASSWORD="$YSQL_PASSWORD" \
        ysqlsh -h "$YSQL_HOST" -p "$YSQL_PORT" -U "$YSQL_USER" -c '\q' 2>/dev/null; do

        if [ $attempt -ge $MAX_ATTEMPTS ]; then
            echo "Failed to connect to YSQL after $((MAX_ATTEMPTS * SLEEP_INTERVAL)) seconds."
            exit 1
        fi

        echo "Attempt $attempt: YSQL not ready, retrying in $SLEEP_INTERVAL seconds..."
        attempt=$((attempt + 1))
        sleep $SLEEP_INTERVAL
    done

    echo "YSQL is ready to accept connections!"

    echo "YSQL is up. Running SQL script..."
    ${pkgs.kubectl}/bin/kubectl exec --kubeconfig=/var/lib/rancher/k3s/server/cred/admin.kubeconfig \
      -n default -it yb-tserver-0 -- env PGPASSWORD="yugabyte" \
      ysqlsh -h yb-tserver-0.yb-tservers.default \
            --username=yugabyte \
            --port=5433 \
            -f - < "${config.sops.templates."yugabyte-init-script-cm.sql".path}"
    echo "SQL script executed successfully."
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

  systemd.services.create-grafana-helm-values = {
    enable = true;
    description = "Create Grafana Helm values Secret from runtime file";
    
    # We need the network and the k3s server to be running.
    after = [ "network-online.target" "k3s.service" "ensure-default-sa.service" ];
    wants = [ "network-online.target" "k3s.service" ];
    requires = [ "k3s.service" "ensure-default-sa.service" ];
    wantedBy = [ "multi-user.target" ];
    
    # Make kubectl available to our script
    path = [ pkgs.kubectl ];

    serviceConfig = {
      Type = "oneshot";
      RemainAfterExit = true;
      ExecStart = "${grafanaHelmValuesScript}/bin/grafanaHelmValuesScript";
    };
  };

  systemd.services.create-ysql-values = {
    enable = true;
    description = "Create ysql Secret from runtime file";
    
    # We need the network and the k3s server to be running.
    after = [ "network-online.target" "k3s.service" "ensure-default-sa.service" ];
    wants = [ "network-online.target" "k3s.service" ];
    requires = [ "k3s.service" "ensure-default-sa.service" ];
    wantedBy = [ "multi-user.target" ];
    
    # Make kubectl available to our script
    path = [ pkgs.kubectl ];

    serviceConfig = {
      Type = "oneshot";
      RemainAfterExit = true;
      ExecStart = "${ybSecretGenerate}/bin/ybSecretGenerate";
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
    pkgs.kubernetes-helm
    pkgs.bash
    pkgs.postgresql

    # pkgs.docker
  ];

  networking.firewall = {
    enable = true;
    allowedTCPPortRanges= [
      { from = 31721; to = 31723; }
      { from = 31701; to = 31709; }
    ];
    allowedTCPPorts = [ 80 443 1235 31895 ];
    allowPing = true;
  };
}
