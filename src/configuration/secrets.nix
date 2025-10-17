{ config, pkgs, ... }:
let
  defaultPasswordPath = config.sops.secrets.password.path;
  kubeTokenPath = config.sops.secrets."kubernetes/token".path;
in
{
  sops = {
    age.keyFile = "/etc/sops/age/cetus.txt";
    defaultSopsFile = ../../secrets.enc.yaml;

    secrets = {
      password = { neededForUsers = true; };
      "kubernetes/token"  = {};
    };
  };

  environment.variables = {
    DEFAULT_PASSWORD_PATH = defaultPasswordPath;
    KUBE_TOKEN_PATH = kubeTokenPath;
  };
}
