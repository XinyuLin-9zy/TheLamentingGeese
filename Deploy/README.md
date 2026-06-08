# AssetBundle deployment workspace

Generated AssetBundle deployment files are placed under:

```text
Deploy/AssetServer/
```

That directory is intentionally ignored by Git because it can contain multi-GB platform bundles.

Typical local test flow:

```powershell
.\Tools\Deployment\Prepare-AssetBundleDeploy.ps1 -SourceRoot .\BuildAssetBundles -Version 1.0.0 -Clean
.\Tools\Deployment\Start-LocalAssetServer.ps1 -Port 8000
```

On macOS/Linux:

```bash
Tools/Deployment/prepare-asset-bundle-deploy.sh --source-root BuildAssetBundles --version 1.0.0 --clean
Tools/Deployment/start-local-asset-server.sh --port 8000
```

For terminal sessions that need the server to stay attached, use:

```bash
Tools/Deployment/start-local-asset-server.sh --port 8000 --foreground
```

Then set UTAGE `ServerUrl` to:

```text
http://<computer-lan-ip>:8000/theweepingswan/1.0.0
```

For Cloudflare R2 upload after login. Replace `YOUR_BUCKET_NAME` with your own bucket:

```powershell
wrangler login
.\Tools\Deployment\Upload-R2.ps1 -Bucket YOUR_BUCKET_NAME -LocalRoot .\Deploy\AssetServer
```

On macOS/Linux:

```bash
wrangler login
Tools/Deployment/upload-r2.sh --bucket YOUR_BUCKET_NAME --local-root Deploy/AssetServer
```
