{ lib, ... }:
{
  disko.devices = {
    disk.disk1 = {
      device = lib.mkDefault "/dev/sda";
      type = "disk";
      content = {
        type = "gpt";
        partitions = {
          boot = {
            name = "boot";
            size = "1M";
            type = "EF02";
          };
          esp = {
            name = "ESP";
            size = "500M";
            type = "EF00";
            content = {
              type = "filesystem";
              format = "vfat";
              mountpoint = "/boot";
            };
          };
          k3sdata = {
            name = "k3sdata";
            size = "2G";
            type = "8300";
            content = {
              type = "filesystem";
              format = "ext4";
              mountpoint = "/k3sdata";
              mountOptions = [
                "defaults"
              ];
            };
          };
          root = {
            name = "root";
            size = "100%";
            content = {
              type = "lvm_pv";
              vg = "pool";
            };
          };
        };
      };
    };
    disk.disk2 = {
      device = lib.mkDefault "/dev/sdb";
      type = "disk";
      content = {
        type = "lvm_pv";
        vg = "db";
      };
    };

    lvm_vg = {
      pool = {
        type = "lvm_vg";
        lvs = {
          root = {
            size = "100%FREE";
            content = {
              type = "filesystem";
              format = "ext4";
              mountpoint = "/";
              mountOptions = [
                "defaults"
              ];
            };
          };
        };
      };
      db = {
        type = "lvm_vg";
        lvs = {
          db_0 = {
            size = "100%FREE";
            content = {
              type = "filesystem";
              mountpoint = "/db";
              # format = "ext4";
              mountOptions = [
                "defaults"
              ];
            };
          };
        };
      };
    };
  };

  fileSystems."/k3sdata/secrets" = {
    device = "/run/secrets";
    options = [ "bind" ];
  };
}