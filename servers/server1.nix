{ kubenixconfig, k3stoken, ... }:
{
  imports = [ ./base-server.nix ];
  networking.hostName = "server1";

  time.timeZone = "Europe/Berlin";


  services.k3s = {
    enable = true;
    role = "server";
    token = k3stoken;
    clusterInit = true;
    extraFlags = toString [
      "--https-listen-port=6444"
      "--bind-address=0.0.0.0"
    ];
  };
}
