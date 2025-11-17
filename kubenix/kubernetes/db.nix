{ kubenix, config, ... }:
{
  imports = [
    kubenix.modules.k8s
  ];

  kubernetes.resources = {
    # Service to expose YugabyteDB ports
    services.yb-master-custom = {
      metadata.labels.app = "yb-master-custom";
      spec = {
        selector.app = "yb-master";
        type = "NodePort";
        ports = [
          { name = "http-ui"; port = 7000; nodePort = 31721; protocol = "TCP"; }
          { name = "tcp-rpc-port"; port = 7100; nodePort = 31722; protocol = "TCP"; }
          { name = "yugabyted-ui"; port = 15433; nodePort = 31723; protocol = "TCP"; }
        ];
      };
    };

    services.yb-tserver-custom = {
      metadata.labels.app = "yb-tserver-custom";
      spec = {
        selector.app = "yb-tserver";
        type = "NodePort";
        ports = [
          { name = "http-ui"; port = 9000; nodePort = 31701; protocol = "TCP"; }
          { name = "http-ycql-met"; port = 12000; nodePort = 31702; protocol = "TCP"; }
          { name = "http-yedis-met"; port = 11000; nodePort = 31703; protocol = "TCP"; }
          { name = "http-ysql-met"; port = 13000; nodePort = 31704; protocol = "TCP"; }

          { name = "tcp-rpc-port"; port = 9100; nodePort = 31705; protocol = "TCP"; }
          { name = "tcp-yedis-port"; port = 6379; nodePort = 31706; protocol = "TCP"; }
          { name = "tcp-yql-port"; port = 9042; nodePort = 31707; protocol = "TCP"; }
          { name = "tcp-ysql-port"; port = 5433; nodePort = 31708; protocol = "TCP"; }

          { name = "yugabyted-ui"; port = 15433; nodePort = 31709; protocol = "TCP"; }
        ];
      };
    };
  };
}
