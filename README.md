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
