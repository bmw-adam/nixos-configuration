{
  kubenix,
  config,
  tpvsel,
  pkgs,
  ...
}:
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
                  "cd ${tpvsel.packages.${pkgs.system}.default}/bin/ && exec ${
                    tpvsel.packages.${pkgs.system}.default
                  }/bin/TpvVyber"
                ];

                env = [
                  {
                    name = "OAUTH_CLIENT";
                    value = "/k3sdata/secrets/oauthClient";
                  }
                  {
                    name = "OTEL_EXPORTER_OTLP_ENDPOINT";
                    value = "https://otlp-gateway-prod-eu-west-2.grafana.net/otlp";
                  }
                  {
                    name = "GRAFANA_OTEL_HEADERS_PATH";
                    value = "/k3sdata/secrets/grafanaOtelHeaders";
                  }
                  {
                    name = "ASPNETCORE_ENVIRONMENT";
                    value = "Development";
                  }
                  {
                    name = "TLS_PFX_KEY";
                    value = "/k3sdata/secrets/pfxKey";
                  }
                  {
                    name = "TLS_PFX_FILE";
                    value = "/k3sdata/secrets/pfxFile";
                  }
                  {
                    name = "YSQL_PASSWORD";
                    value = "/k3sdata/secrets/ysqlPassword";
                  }
                ];

                ports = [
                  {
                    name = "tpvsel";
                    containerPort = 1234;
                    protocol = "TCP";
                  }
                  {
                    name = "tpvsel";
                    containerPort = 1235;
                    protocol = "TCP";
                  }
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
        type = "ClusterIP";
        ports = [
          {
            name = "tpvsel";
            port = 1234;
            targetPort = 1234;
            protocol = "TCP";
          }
        ];
      };
    };

    ingresses.tpvsel = {
      metadata = {
        name = "tpvsel-ingress";
        annotations = {
          # Standard Traefik entrypoint
          "kubernetes.io/ingress.class" = "traefik";
          # Force traffic to standard http (port 80)
          "traefik.ingress.kubernetes.io/router.entrypoints" = "web";
        };
      };
      spec = {
        rules = [
          {
            # If you want this to work via IP address (e.g. http://192.168.1.50)
            # instead of a domain name, simply remove the "host =" line below.
            # host = "tpvsel.local"; 
            http = {
              paths = [
                {
                  path = "/";
                  pathType = "Prefix";
                  backend = {
                    service = {
                      name = "tpvsel";
                      port = {
                        number = 1234; # Routes traffic from Port 80 -> Service Port 1234
                      };
                    };
                  };
                }
              ];
            };
          }
        ];
      };
    };
  };
}
