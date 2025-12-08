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
          }
        ];
      };
    };

    manifests.yugabyte-tls-auth-enabled.content = {
      apiVersion = "helm.cattle.io/v1";
      kind = "HelmChart";
      metadata = {
        name = "yugabytedb";
        namespace = "default";
      };

      spec = {
        targetNamespace = "default";
        createNamespace = true;
        repo = "https://charts.yugabyte.com";
        chart = "yugabyte";
        version = "2025.1.1";

        valuesContent = ''
          replication:
            factor: 1
          replicas:
            master: 1
            tserver: 1

          storage:
            master:
              count: 1
              size: "10Gi"
              storageClass: "local-path"
              mountPath: "/db"
            tserver:
              count: 1
              size: "10Gi"
              storageClass: "local-path"
              mountPath: "/db"

          gflags:
            tserver:
              ysql_enable_auth: true
              ysql_server_cert_file: "/k3sdata/secrets/yugabyteServerCrt"
              ysql_server_key_file: "/k3sdata/secrets/yugabyteServerKey"
              ysql_ca_cert_file: "/k3sdata/secrets/yugabyteClientCrt"
              ysql_bind_address: "0.0.0.0:5433"
              start_pgsql_proxy: true

          extraVolumes:
            master:
              - name: "k3s-secrets"
                hostPath:
                  path: "/k3sdata/secrets"
                  type: "Directory"
            tserver:
              - name: "k3s-secrets"
                hostPath:
                  path: "/k3sdata/secrets"
                  type: "Directory"

          extraVolumeMounts:
            master:
              - name: "k3s-secrets"
                mountPath: "/k3sdata/secrets"
                readOnly: true
            tserver:
              - name: "k3s-secrets"
                mountPath: "/k3sdata/secrets"
                readOnly: true

          resource:
            master:
              requests:
                cpu: "0.5"
                memory: "0.5Gi"
            tserver:
              requests:
                cpu: "0.5"
                memory: "0.5Gi"

          enableLoadBalancer: false

          extraEnvVars:
            master:
              - name: "YSQL_PASSWORD"
                valueFrom:
                  secretKeyRef:
                    name: "yb-auth-secret"
                    key: "YSQL_PASSWORD"
              - name: "YSQL_USERNAME"
                value: "ysql"
            tserver:
              - name: "YSQL_PASSWORD"
                valueFrom:
                  secretKeyRef:
                    name: "yb-auth-secret"
                    key: "YSQL_PASSWORD"
              - name: "YSQL_USERNAME"
                value: "ysql"
        '';
      };
    };

    images = [
      (pkgs.dockerTools.buildLayeredImage {
        name = "tpvsel";
        contents = [
          tpvsel.packages.${pkgs.system}.default
          pkgs.bash
          pkgs.findutils
          pkgs.coreutils
          pkgs.gnugrep
          pkgs.tzdata
        ];
        tag = "latest";

        # extraCommands = ''
        #   # Set the timezone to Europe/Prague
        #   echo "Europe/Berlin" > /etc/timezone

        #   # Optional: set TZ environment variable for applications
        #   echo 'export TZ=Europe/Berlin' >> /etc/profile
        # '';

        config = {
          Cmd = [ "$TPVSEL_PATH" ];
          
          EntryPoint = ["${pkgs.coreutils}/bin/date"];
          Env = ["TZDIR=${pkgs.tzdata}/share/zoneinfo"];

          ExposedPorts = {
            "1234/tcp" = {};
            "1235/tcp" = {};
          };
        };
      })
    ];
  };
}
