{ kubenixconfig, config, ... }:
let
  kubeTokenPath = config.sops.secrets."kubernetes/token".path;
in
{
  imports = [ ./base-server.nix ];
  networking.hostName = "server1";

  time.timeZone = "Europe/Berlin";

  services.k3s = {
    enable = true;
    role = "server";
    tokenFile = kubeTokenPath;
    clusterInit = true;
    extraFlags = toString [
      "--https-listen-port=6444"
      "--bind-address=0.0.0.0"
    ];
  };
}
