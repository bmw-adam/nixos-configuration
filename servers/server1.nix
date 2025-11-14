{ kubenixconfig, config, pkgs, tpvsel, ... }:
let
  # defaultPasswordPath = config.sops.secrets.password.path;
  kubeTokenPath = config.sops.secrets."kubernetes/token".path;
  # tlsKeyPath = config.sops.secrets.tlsKey.path;
  # tlsCrtPath = config.sops.secrets.tlsCrt.path;
  # distDirPath = "${tpvsel.packages.${pkgs.system}.default}/bin/dist";
in
{
  imports = [ ./base-server.nix ];
  networking.hostName = "server1";

  time.timeZone = "Europe/Berlin";

  # environment.variables.DIST_DIR_PATH = "${tpvsel.packages.${pkgs.system}.default}/bin/dist";
  # environment.variables.TPVSEL_PATH = "${tpvsel.packages.${pkgs.system}.default}/bin/backend";
    #   DEFAULT_PASSWORD_PATH = defaultPasswordPath;
    # KUBE_TOKEN_PATH = kubeTokenPath;
    # TLS_KEY = kubeTokenPath;
    # TLS_CRT = tlsCrtPath;

  services.k3s = {
    enable = true;
    role = "server";
    tokenFile = kubeTokenPath;
    clusterInit = true;

    extraFlags = toString [
      "--https-listen-port=6444"
      "--bind-address=0.0.0.0"
      "--disable=traefik"
    ];

    manifests.grafana.content = {
      apiVersion = "helm.cattle.io/v1";
      kind = "HelmChart";
      metadata = {
        name = "grafana-cloud-metrics";
        namespace = "kube-system";
      };

      externalServices = {
        prometheus = {
          host = "prometheus-prod-65-prod-eu-west-2.grafana.net";
          scheme = "https";
        };
      };

      spec = {
        targetNamespace = "kube-system";
        createNamespace = true;
        repo = "https://grafana.github.io/helm-charts";
        chart = "k8s-monitoring";
        version = "3.5.5";
        valuesSecrets = [
          {
            # The name of the Secret to read from
            name = "grafana-helm-values";
            
            # The 'keys' in the Secret to use as values files.
            # Our systemd service will create a key named 'values.yaml'.
            keys = [ "values.yaml" ];
            
            # This defaults to false, which is what we want.
            # It ensures that if the Secret changes, the chart will be updated.
            # ignoreUpdates = false; 
          }
        ];
      };
    };

    images = [
      (pkgs.dockerTools.buildLayeredImage {
        name = "tpvsel";
        contents = [ tpvsel.packages.${pkgs.system}.default pkgs.bash pkgs.findutils pkgs.coreutils pkgs.gnugrep ];
        tag = "latest";
        
        # extraCommands = ''
        #   find / | grep tpvsel
        # '';
        config = {
          Cmd = [ "$TPVSEL_PATH" ];

          ExposedPorts = {
            "1234/tcp" = { };
            "1235/tcp" = { };
          };
        };
      })
    ];
  };
}
