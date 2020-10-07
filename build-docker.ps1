$ErrorActionPreference = "Stop"

$container_name = "shader-playground-build"
$image_name = "shader-playground-build-image"
$docker_project_path = "c:\\src\\shader-playground"
$total_system_ram_gb = (Get-CimInstance Win32_PhysicalMemory | Measure-Object -Property capacity -Sum).sum /1gb

$container_exists = $(docker ps -a -f "name=${container_name}" --format '{{.Names}}') -match "${container_name}"
if(!$container_exists) {
    echo "Building docker image..."
    docker build "-m${total_system_ram_gb}GB" --tag ${image_name} .

    echo "Building docker container..."
    docker create `
        -t -i `
        --name ${container_name} `
        --storage-opt size=120G `
        "-m${total_system_ram_gb}GB" `
        --cpu-count=$env:NUMBER_OF_PROCESSORS `
        ${image_name}
}

$container_is_running = $(docker ps -f "name=${container_name}" --format '{{.Names}}') -match "${container_name}"
if(!$container_is_running) {
    echo "Starting docker container..."
    docker start ${container_name}
}

echo "Building..."
docker exec "${container_name}" `
    "powershell" `
    "./build.ps1" `
    "--always-cache=true" `
    "--Verbosity=Diagnostic"

function copyFromImage($relPath) {
    echo "Copying '${relPath}' from container..."
    if (Test-Path ".\${relPath}") { rm -r ".\${relPath}" }
    docker cp ${container_name}:"${docker_project_path}\${relPath}" ".\${relPath}"
}

echo "Stopping docker container..."
docker stop ${container_name}

# Copy build artifacts out of the container to the host's directory.
echo "Copying build artifacts..."
copyFromImage("build")
copyFromImage("src\ShaderPlayground.Core\bin")
copyFromImage("src\ShaderPlayground.Core\Binaries")
copyFromImage("src\ShaderPlayground.Core.Tests\bin")
