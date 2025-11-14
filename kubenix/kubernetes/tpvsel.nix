{ kubenix, config, tpvsel, pkgs, ... }: 
let
in
{
  imports = [
    kubenix.modules.k8s
  ];

  kubernetes.resources = {
    deployments.tpvsel = {
      metadata = {
        name = "tpvsel";
        labels.app = "tpvsel";
      };

      spec = {
        replicas = 1;

        selector = {
          matchLabels.app = "tpvsel";
        };

        template = {
          metadata = {
            labels.app = "tpvsel";
          };

          spec = {
            hostname = "tpvsel01";

            containers = [
              {
                name = "tpvsel";
                image = "tpvsel:latest";
                imagePullPolicy = "Never";

                command = [
                  "/bin/bash"
                  "-c"
                  "cd ${tpvsel.packages.${pkgs.system}.default}/bin/ && exec ${tpvsel.packages.${pkgs.system}.default}/bin/TpvVyber"
                ];

                env = [
                  # { name = "TLS_KEY"; value = "/k3sdata/secrets/tlsKey"; }
                  # { name = "TLS_CRT"; value = "/k3sdata/secrets/tlsCrt"; }
                  # { name = "DIST_DIR_PATH"; value = "${tpvsel.packages.${pkgs.system}.default}/bin/dist"; }
                  { name = "OAUTH_CLIENT"; value = "/k3sdata/secrets/oauthClient"; }
                  # { name = "GRAFANA_TOKEN"; value = "/k3sdata/secrets/grafanaToken"; }
                  { name = "OTEL_EXPORTER_OTLP_ENDPOINT"; value = "https://otlp-gateway-prod-eu-west-2.grafana.net/otlp"; }
                  { name = "GRAFANA_OTEL_HEADERS_PATH"; value = "/k3sdata/secrets/grafanaOtelHeaders"; }
                  { name = "ASPNETCORE_ENVIRONMENT"; value = "Development"; }
                  { name = "TLS_PFX_KEY"; value = "/k3sdata/secrets/pfxKey"; }
                  { name = "TLS_PFX_FILE"; value = "/k3sdata/secrets/pfxFile"; }
                ];

                ports = [
                  { name = "tpvsel"; containerPort = 1234; protocol = "TCP"; }
                  { name = "tpvsel"; containerPort = 1235; protocol = "TCP"; }
                ];

                volumeMounts = [
                  {
                    name = "k3sdata";
                    mountPath = "/k3sdata";
                  }
                ];
              }
            ];

            volumes = [
              {
                name = "k3sdata";
                hostPath = {
                  path = "/k3sdata";
                  type = "Directory";
                };
              }
            ];
          };
        };
      };
    };
    
    services.tpvsel = {
      metadata.labels.app = "tpvsel";
      spec = {
        selector.app = "tpvsel";
        type = "NodePort";
        ports = [
          { name = "tpvsel"; port = 1235; nodePort = 31895; protocol = "TCP"; }
        ];
      };
    };
  };
}
