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
        namespace = "default";
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
        valuesContent =''
cluster:
  name: TpvSelCluster
destinations:
  - name: grafana-cloud-metrics
    type: prometheus
    url: https://prometheus-prod-65-prod-eu-west-2.grafana.net./api/prom/push
    auth:
      type: basic
      username: "2793474"
      password: "${config.sops.placeholder.grafanaToken}"
  - name: grafana-cloud-logs
    type: loki
    url: https://logs-prod-012.grafana.net./loki/api/v1/push
    auth:
      type: basic
      username: "1392415"
      password: "${config.sops.placeholder.grafanaToken}"
  - name: gc-otlp-endpoint
    type: otlp
    url: https://otlp-gateway-prod-eu-west-2.grafana.net./otlp
    protocol: http
    auth:
      type: basic
      username: "1434621"
      password: "${config.sops.placeholder.grafanaToken}"
    metrics:
      enabled: true
    logs:
      enabled: true
    traces:
      enabled: true
clusterMetrics:
  enabled: true
  opencost:
    enabled: true
    metricsSource: grafana-cloud-metrics
    opencost:
      exporter:
        defaultClusterId: TpvSelCluster
      prometheus:
        existingSecretName: grafana-cloud-metrics-grafana-cloud-metrics-k8s-monitoring
        external:
          url: https://prometheus-prod-65-prod-eu-west-2.grafana.net./api/prom
  kepler:
    enabled: true
clusterEvents:
  enabled: true
podLogs:
  enabled: true
applicationObservability:
  enabled: true
  receivers:
    otlp:
      grpc:
        enabled: true
        port: 4317
      http:
        enabled: true
        port: 4318
    zipkin:
      enabled: true
      port: 9411
integrations:
  alloy:
    instances:
      - name: alloy
        labelSelectors:
          app.kubernetes.io/name:
            - alloy-metrics
            - alloy-singleton
            - alloy-logs
            - alloy-receiver
alloy-metrics:
  enabled: true
alloy-singleton:
  enabled: true
alloy-logs:
  enabled: true
alloy-receiver:
  enabled: true
  alloy:
    extraPorts:
      - name: otlp-grpc
        port: 4317
        targetPort: 4317
        protocol: TCP
      - name: otlp-http
        port: 4318
        targetPort: 4318
        protocol: TCP
      - name: zipkin
        port: 9411
        targetPort: 9411
        protocol: TCP
        '';
      };
    };

    # Enable Helm controller (optional if you just want Helm CLI)
    # extraConfig = ''
    #   write-kubeconfig-mode 644
    # '';

    # Optional: install helm plugin or CLI in the system
    # packages = [ pkgs.helm ];

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
