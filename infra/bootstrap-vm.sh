#!/usr/bin/env bash
# Reproducible DEI Ubuntu VM bootstrap script (CON-DEP-02, CON-DEP-03).
# Run once as sudo on both Development and Production VMs:
#   sudo bash infra/bootstrap-vm.sh <dev|prod>

set -euo pipefail

ENVIRONMENT="${1:-dev}"
if [[ "$ENVIRONMENT" != "dev" && "$ENVIRONMENT" != "prod" ]]; then
  echo "Usage: sudo bash infra/bootstrap-vm.sh <dev|prod>" >&2
  exit 1
fi

echo "==> Installing base utilities and Docker Engine + Compose v2..."
apt-get update -y
apt-get install -y ca-certificates curl gnupg ufw jq

install -m 0755 -d /etc/apt/keyrings
if [ ! -f /etc/apt/keyrings/docker.asc ]; then
  curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
  chmod a+r /etc/apt/keyrings/docker.asc
fi

. /etc/os-release
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/ubuntu \
  ${VERSION_CODENAME} stable" > /etc/apt/sources.list.d/docker.list

apt-get update -y
apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
systemctl enable --now docker

echo "==> Creating application directories..."
mkdir -p /opt/metro-core /opt/metro-web
if [ -n "${SUDO_USER:-}" ]; then
  usermod -aG docker "$SUDO_USER" || true
  chown -R "$SUDO_USER:$SUDO_USER" /opt/metro-core /opt/metro-web
fi

echo "==> Configuring UFW firewall (SSH, HTTP, HTTPS)..."
ufw allow OpenSSH
ufw allow 80/tcp
ufw allow 443/tcp
if [ "$ENVIRONMENT" = "dev" ]; then
  ufw allow 4200/tcp
  ufw allow 8080/tcp
fi
ufw --force enable

cat <<EOF

Bootstrap completed for environment: ${ENVIRONMENT}

Next manual steps (secrets never stored in Git — CON-DEP-05):
1. Create /opt/metro-core/.env.${ENVIRONMENT} (based on metro-core/.env.example)
2. Create /opt/metro-web/.env.${ENVIRONMENT} (WEB_HOST_PORT, API_UPSTREAM)
3. Register GitHub Actions self-hosted runner (Settings -> Actions -> Runners -> New self-hosted runner)
   with labels: self-hosted,linux,${ENVIRONMENT}
   then install as a systemd service:
     sudo ./svc.sh install
     sudo ./svc.sh start
EOF
