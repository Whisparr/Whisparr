#!/usr/bin/env python3
import os
import sys
import time
import jwt
import requests


def main():
    try:
        app_id = os.environ['CI_APP_ID']
        private_key_path = '/tmp/app_private_key.pem'
        with open(private_key_path, 'r') as fh:
            private_key = fh.read()

        now = int(time.time())
        payload = {'iat': now - 60, 'exp': now + (9 * 60), 'iss': int(app_id)}
        jwt_token = jwt.encode(payload, private_key, algorithm='RS256')

        owner, repo = os.environ['GITHUB_REPOSITORY'].split('/')
        headers = {'Authorization': f'Bearer {jwt_token}', 'Accept': 'application/vnd.github+json'}

        # Get installation ID for this repo (works for org or repo installations)
        resp = requests.get(f'https://api.github.com/repos/{owner}/{repo}/installation', headers=headers)
        resp.raise_for_status()
        installation_id = resp.json()['id']

        resp2 = requests.post(f'https://api.github.com/app/installations/{installation_id}/access_tokens', headers=headers)
        resp2.raise_for_status()
        token_value = resp2.json()['token']

        # Prefer writing directly to GITHUB_ENV so the token is available to subsequent
        # steps without using stdout. This avoids printing the token.
        env_path = os.environ.get('GITHUB_ENV')
        if env_path:
            try:
                with open(env_path, 'a') as out:
                    out.write(f"RELEASE_TOKEN={token_value}\n")
                    out.write(f"INSTALLATION_ID={installation_id}\n")
                try:
                    os.chmod(env_path, 0o600)
                except Exception:
                    pass
            except Exception:
                # Fall back to writing to /tmp if GITHUB_ENV isn't available
                with open('/tmp/mint_out.env', 'w') as out:
                    out.write(f"release_token={token_value}\n")
                    out.write(f"installation_id={installation_id}\n")
                try:
                    os.chmod('/tmp/mint_out.env', 0o600)
                except Exception:
                    pass

    except Exception as exc:
        print('Error creating installation token:', exc, file=sys.stderr)
        sys.exit(1)


if __name__ == '__main__':
    main()
