{ config, pkgs, ... }:
let
  defaultPasswordPath = config.sops.secrets.password.path;
  kubeTokenPath = config.sops.secrets."kubernetes/token".path;
  oIdcSecretPath = config.sops.secrets.oIdcSecret.path;
  grafanaTokenPath = config.sops.secrets.grafanaToken.path;
  grafanaOtelHeadersPath = config.sops.secrets.grafanaOtelHeaders.path;
  pfxKeyPath = config.sops.secrets.pfxKey.path;
  pfxFilePath = config.sops.secrets.pfxFile.path;
  grafanaCloudMetricsPath = config.sops.templates."grafana-cloud-metrics.yaml".path;
  ysqlPasswordPath = config.sops.secrets.ysqlPassword.path;
  yugabyteServerCrtPath = config.sops.secrets.yugabyteServerCrt.path;
  yugabyteServerKeyPath = config.sops.secrets.yugabyteServerKey.path;
  yugabyteClientCrtPath = config.sops.secrets.yugabyteClientCrt.path;
in
{
  sops = {
    age.keyFile = "/etc/sops/age/cetus.txt";
    defaultSopsFile = ../../secrets/default.yaml;

    secrets = {
      password = { neededForUsers = true; };
      "kubernetes/token"  = {};
      oIdcSecret = {};
      grafanaToken = {};
      grafanaOtelHeaders = {};
      pfxKey = {};
      pfxFile = {
        format = "binary";
        sopsFile = ../../secrets/pfxCrt.yaml;
      };
      ysqlPassword = {};
      yugabyteServerCrt = {};
      yugabyteServerKey = {};
      yugabyteClientCrt = {};
    };

    templates = {
      "grafana-cloud-metrics.yaml" = {
        content = ''
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
      "yugabyte-init-script-cm.sql" = {
        content = ''
          -- 1. Create the database
          CREATE DATABASE tpvvyberdb;

          -- 2. Create the application user
          CREATE USER tpv WITH PASSWORD '${config.sops.placeholder.ysqlPassword}';

          -- 3. Grant privileges on the database
          GRANT ALL PRIVILEGES ON DATABASE tpvvyberdb TO tpv;


          -- 5. Connect to the new database to create the schema
          \c tpvvyberdb yugabyte 
          -- or whichever superuser youre connected as

          -- 6. Create a dedicated schema for your application
          CREATE SCHEMA tpv_schema;

          -- 7. Grant privileges to your user on the schema
          GRANT USAGE, CREATE ON SCHEMA tpv_schema TO tpv;

          ALTER DEFAULT PRIVILEGES IN SCHEMA tpv_schema GRANT ALL ON TABLES TO tpv;

          -- 4. Disable the built-in 'yugabyte' user (optional)
          ALTER USER yugabyte WITH NOLOGIN;
        '';
      };
    };
  };


  environment.variables = {
    DEFAULT_PASSWORD_PATH = defaultPasswordPath;
    KUBE_TOKEN_PATH = kubeTokenPath;
    GRAFANA_TOKEN_PATH = grafanaTokenPath;
    GRAFANA_OTEL_HEADERS_PATH = grafanaOtelHeadersPath;
    PFX_KEY = pfxKeyPath;
    PFX_FILE = pfxFilePath;
    GRAFANA_CLOUD_METRICS_TEMPLATE_PATH = grafanaCloudMetricsPath;
    YSQL_PASSWORD = ysqlPasswordPath;
    YUGABYTE_SERVER_CRT = yugabyteServerCrtPath;
    YUGABYTE_SERVER_KEY = yugabyteServerKeyPath;
    YUGABYTE_CLIENT_CRT = yugabyteClientCrtPath;
    OIDC_SECRET = oIdcSecretPath;
  };
}
