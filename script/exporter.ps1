# A script to help the workflow of balancers. It copies files to the game,
# run cRPG so they can run the export commands, and copies the files back
# to the repo.

$ErrorActionPreference = "Stop"

if (-not $env:MB_CLIENT_PATH) {
    Write-Error "MB_CLIENT_PATH environment variable is not set."
    exit 1
}

$repoPath = Split-Path -Parent $PSScriptRoot
$repoModuleDataPath = Join-Path $repoPath "src\Module.Server\ModuleData"
$gameModulePath = Join-Path $env:MB_CLIENT_PATH "Modules\cRPG"
$gameModuleDataPath = Join-Path $gameModulePath "ModuleData"

$xmlFiles = @(
    "action_sets.xml",
    "action_types.xml",
    "collision_infos.xml",
    "crafting_pieces.xml",
    "crafting_templates.xml",
    "item_holsters.xml",
    "item_usage_sets.xml",
    "physics_materials.xml",
    "weapon_descriptions.xml",
    "items\arm_armors.xml",
    "items\banners.xml",
    "items\body_armors.xml",
    "items\head_armors.xml",
    "items\horses_and_others.xml",
    "items\leg_armors.xml",
    "items\shields.xml",
    "items\shoulder_armors.xml",
    "items\weapons.xml"
)

Write-Host "Copying XMLs to game..." -ForegroundColor Cyan
foreach ($xml in $xmlFiles) {
    $src = Join-Path $repoModuleDataPath $xml
    $dst = Join-Path $gameModuleDataPath $xml
    $dstDir = Split-Path $dst -Parent
    Copy-Item $src $dst -Force
    Write-Host "  $xml"
}

$exe = Join-Path $env:MB_CLIENT_PATH "bin\Win64_Shipping_Client\Bannerlord.exe"
$gameArgs = "_MODULES_*Native*Multiplayer*cRPG*_MODULES_ /multiplayer"
Write-Host "`nStarting Bannerlord, use ALT+~ to open the console in-game, all commands start with 'crpg.'..." -ForegroundColor Green
$process = Start-Process -FilePath $exe -ArgumentList $gameArgs -WorkingDirectory (Split-Path $exe -Parent) -PassThru
$process.WaitForExit()
Write-Host "Bannerlord exited (code $($process.ExitCode))." -ForegroundColor Green

Write-Host "`nCopying XMLs + items.json + thumbnails back to repo..." -ForegroundColor Cyan
foreach ($xml in $xmlFiles) {
    $src = Join-Path $gameModuleDataPath $xml
    $dst = Join-Path $repoModuleDataPath $xml
    Copy-Item $src $dst -Force
    Write-Host "  $xml"
}

$itemsJsonSrc = Join-Path $gameModuleDataPath "items.json"
if (Test-Path $itemsJsonSrc) {
    Move-Item $itemsJsonSrc (Join-Path $repoModuleDataPath "items.json") -Force
    Write-Host "  items.json"
}

$thumbnailSrc = Join-Path $gameModuleDataPath "item-thumbnails"
if (Test-Path $thumbnailSrc) {
    $thumbnailDst = Join-Path $repoPath "src\WebUI\public"
    if (-not (Test-Path $thumbnailDst)) { New-Item -ItemType Directory -Path $thumbnailDst -Force | Out-Null }
    Get-ChildItem $thumbnailSrc -File | ForEach-Object {
        Move-Item $_.FullName (Join-Path $thumbnailDst $_.Name) -Force
    }
    Write-Host "  item thumbnails"
}

Write-Host "`nDone." -ForegroundColor Green
