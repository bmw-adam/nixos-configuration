{ kubenixconfig, config, pkgs, tpvsel, ... }:
let
  defaultPasswordPath = config.sops.secrets.password.path;
  kubeTokenPath = config.sops.secrets."kubernetes/token".path;
  tlsKeyPath = config.sops.secrets.tlsKey.path;
  tlsCrtPath = config.sops.secrets.tlsCrt.path;
  distDirPath = "${tpvsel.packages.${pkgs.system}.default}/bin/dist";
in
{
  imports = [ ./base-server.nix ];
  networking.hostName = "server1";

  time.timeZone = "Europe/Berlin";

  environment.variables.DIST_DIR_PATH = "${tpvsel.packages.${pkgs.system}.default}/bin/dist";
    #   DEFAULT_PASSWORD_PATH = defaultPasswordPath;
    # KUBE_TOKEN_PATH = kubeTokenPath;
    # TLS_KEY = kubeTokenPath;
    # TLS_CRT = tlsCrtPath;

  systemd.services.tpvsel = {
    enable = true;
    description = "Tpv sel service";

    after = [ "network-online.target" "k3s.service" "ensure-default-sa.service" ];
    wantedBy = [ "multi-user.target" ];

    serviceConfig = {
      Type = "oneshot";
      RemainAfterExit = true;
      ExecStart = "${tpvsel.packages.${pkgs.system}.default}/bin/backend";
      Environment = [ "TLS_KEY=\"${tlsKeyPath}\"" "DIST_DIR_PATH=\"${distDirPath}\"" "TLS_CRT=\"${tlsCrtPath}\"" ];
    };
  };

  services.k3s = {
    enable = true;
    role = "server";
    tokenFile = kubeTokenPath;
    clusterInit = true;
    extraFlags = toString [
      "--https-listen-port=6444"
      "--bind-address=0.0.0.0"
    ];
    images = [
      (pkgs.dockerTools.buildLayeredImage {
        name = "tpvsel";
        contents = [ pkgs.nginx ];
        tag = "latest";
        
        extraCommands = ''
          mkdir -p etc
          chmod u+w etc
          echo "nginx:x:1000:1000::/:" > etc/passwd
          echo "nginx:x:1000:nginx" > etc/group
        '';
        config = {
          Cmd = [ "nginx" "-c" "/etc/nginx/nginx.conf" ];
          ExposedPorts = {
            # "80/tcp" = { };
          };
        };
      })
    ];
  };
}
