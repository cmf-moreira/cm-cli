# Initialize a Project

This tutorial walks you through initializing a new CM MES project using the `cmf init` command.

## 1. Create the project folder

Create a folder to store your new customization project files.

!!! warning

    On Windows, some applications and libraries do not support file paths longer than 256 characters. 
    CM MES customization projects have long file paths. To avoid problems on Windows OS, you should:

    * Use short project file names;
    * Initialize your projects on a folder as nearest as possible to the filesystem drive root.

## 2. Open PowerShell command line

Open a PowerShell terminal and navigate to your new project folder.

## 3. Check Node.js and NPM Version

Run the following commands to check your current versions:

``` powershell
# Check Node.js version
node -v

#Check NPM version
npm -v
```

Validate that their versions match the compatibility list stated in the [installation guide](../../../../01-install/index.md).
If needed, use `nvm` command to fix it.

## 4. Initialize Project

Execute the `cmf init` command with the desired project name and required arguments.

!!! note

    All the dependencies versions and command arguments are detailed on the [Project Types Concept](../../../concepts/project-types/index.md) page.

=== "MES v10 or above"

    ``` powershell
    cmf init ExampleProject `
        --version 1.0.0 `
        --infra ..\config\infra.json `
        --config ..\config\env.json `
        --MESVersion 11.0.0 `
        --ngxSchematicsVersion 11.0.0 `
        --nugetVersion 11.0.0 `
        --testScenariosNugetVersion 11.0.0 `
        --deploymentDir \\directory\Deployments `
        --ISOLocation \\directory\CriticalManufacturing.iso
    ```

=== "MES v9 or below"

    ``` powershell
    cmf init ExampleProject `
        --version 1.0.0 `
        --infra ..\config\infra.json `
        --config ..\config\ExampleEnvironment.json `
        --MESVersion 9.0.11 `
        --DevTasksVersion 9.0.4 `
        --HTMLStarterVersion 8.0.0 `
        --yoGeneratorVersion 8.1.1 `
        --nugetVersion 9.0.11 `
        --testScenariosNugetVersion 9.0.11 `
        --deploymentDir \\vm-project\Deployments `
        --ISOLocation \\setups\CriticalManufacturing.iso
    ```

=== "MES App"

    ``` powershell
    cmf init ExampleProject `
        --version 1.0.0 `
        --infra ..\config\infra.json `
        --config ..\config\ExampleEnvironment.json `
        --MESVersion 11.0.0 `
        --ngxSchematicsVersion 11.0.0 `
        --nugetVersion 11.0.0 `
        --testScenariosNugetVersion 11.0.0 `
        --deploymentDir \\directory\Deployments `
        --appName "My App" `
        --appId "MyApp" `
        --appAuthor "Critical Manufacturing" `
        --appDescription "My First App" `
        --appTargetFramework 11.0.0 `
        --appLicensedApplication "My App" `
        --repositoryType "App"
    ```

## 5. Review the created project structure

The `cmf init` command should have terminated with success and created a basic project structure similar to:

``` log
📦ExampleProject
 ┣ 📂.config               # Dotnet tools configuration
 ┃ ┗ 📜dotnet-tools.json
 ┣ 📂EnvironmentConfigs    # Environments configuration repository
 ┃ ┗ 📜ExampleEnvironment.json
 ┣ 📂Libs                  # External libs dependencies (binaries)
 ┃ ┗ 📂...
 ┣ 📜.gitignore            # Spec files to ignore
 ┣ 📜.project-config.json  # Project configuration used during scaffolding
 ┣ 📜cmfpackage.json       # Project root package
 ┣ 📜global.json           # Dotnet global.json
 ┣ 📜NuGet.Config          # NuGet repository configuration
 ┗ 📜repositories.json     # The build/release repositories configuration
 ```

!!! note

    The initial project structure may vary, depending on the CM CLI
    version and the project type selected (`--repositoryType` argument).

## 6. Validate `repositories.json`

Verify the `repositories.json` file in the project root folder conforms to the [specification](../../../../03-explore/config-files/repositories.json/index.md).

## 7. Add LBOs SDK

Store your environment's LBOs in the `Libs\LBOs` directory of your project.

## 8. Store project on source control

Use a source control system (like Git) to manage your project versions.

Store the result of `cmf init` in the source control.

!!! note

    The CM CLI assumes that you are using `git`. If that is not
    the case, adapt `.gitignore` files to your source control system.