# TPV - SELECT
## NIXOS CONFIG
* [~/.config/sops/age/cetus.txt](~/.config/sops/age/cetus.txt) PK required
* [~/.ssh/id_ed25519_mpbox](~/.ssh/id_ed25519_mpbox) ssh PK required

* Hetzner Deployment
```bash
chmod +x deploy.sh
./deploy.sh
ssh -i ~/.ssh/id_ed25519_mpbox root@78.46.206.4
```

Checkout the [flake.nix](flake.nix) for examples tested on different hosters.

[root@server1:~]# kubectl logs example --kubeconfig=/var/lib/rancher/k3s/server/cred/admin.kubeconfig
Error from server (BadRequest): container "nginx" in pod "example" is waiting to start: trying and failing to pull image

[root@server1:~]# kubectl  --kubeconfig=/var/lib/rancher/k3s/server/cred/admin.kubeconfig get pods
NAME      READY   STATUS             RESTARTS   AGE
example   0/1     ImagePullBackOff   0          2m59s

[root@server1:~]# ctr --namespace k8s.io images list | grep tpv
[root@server1:~]# ctr --namespace k8s.io run --rm docker.io/library/tpvsel:yivxzk47n7ci255az583f29mlr97k45l test
ctr: failed to create shim task: OCI runtime create failed: runc create failed: unable to start container process: error during container init: exec: "/bin/tpvsel": stat /bin/tpvsel: no such file or directory
