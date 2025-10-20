{ kubenixconfig, config, pkgs, tpvsel, ... }:
let
  kubeTokenPath = config.sops.secrets."kubernetes/token".path;
in
{
  imports = [ ./base-server.nix ];
  networking.hostName = "server1";

  time.timeZone = "Europe/Berlin";

  systemd.services.tpvsel = {
    enable = true;
    description = "Tpv sel service";

    after = [ "network-online.target" "k3s.service" "ensure-default-sa.service" ];
    wantedBy = [ "multi-user.target" ];

    serviceConfig = {
      Type = "oneshot";
      RemainAfterExit = true;
      ExecStart = "${tpvsel.packages.${pkgs.system}.default}/bin/backend";
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
        
        extraCommands = ''
          mkdir -p etc
          chmod u+w etc
          echo "nginx:x:1000:1000::/:" > etc/passwd
          echo "nginx:x:1000:nginx" > etc/group
        '';
        config = {
          Cmd = [ "nginx" "-c" "/etc/nginx/nginx.conf" ];
          ExposedPorts = {
            "80/tcp" = { };
          };
        };
      })
    ];
  };
}
