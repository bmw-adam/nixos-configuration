{ config, pkgs, ... }:
let
  defaultPasswordPath = config.sops.secrets.password.path;
  kubeTokenPath = config.sops.secrets."kubernetes/token".path;
  tlsKeyPath = config.sops.secrets.tlsKey.path;
  tlsCrtPath = config.sops.secrets.tlsCrt.path;
in
{
  sops = {
    age.keyFile = "/etc/sops/age/cetus.txt";
    defaultSopsFile = ../../secrets.enc.yaml;

    secrets = {
      password = { neededForUsers = true; };
      "kubernetes/token"  = {};
      tlsCrt = {};
      tlsKey = {};
    };
  };

  environment.variables = {
    DEFAULT_PASSWORD_PATH = defaultPasswordPath;
    KUBE_TOKEN_PATH = kubeTokenPath;
    TLS_KEY = kubeTokenPath;
    TLS_CRT = tlsCrtPath;
  };
}
