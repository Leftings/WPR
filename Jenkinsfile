pipeline {
    agent none  // We'll specify individual agents for different stages

    environment {
        SOLUTION_NAME = 'WPR.sln'  // Replace with your actual solution name
    }

    options {
        // Timeout for the entire pipeline
        timeout(time: 30, unit: 'MINUTES')
    }

    stages {
        stage('Checkout') {
            agent any  // Jenkins will automatically pick an available agent
            steps {
                // Checkout your code from the repository
                checkout scm
            }
        }

        stage('Setup .NET Core SDK') {
            agent any
            steps {
                script {
                    // Install .NET Core SDK (Note: You can specify OS-specific logic here if needed)
                    if (isUnix()) {
                        sh 'curl -sSL https://aka.ms/install-dotnet.sh | bash' // For Ubuntu/macOS
                    } else {
                        bat 'choco install dotnetcore-sdk --version 8.0' // For Windows
                    }
                }
            }
        }

        stage('Cache NuGet Packages') {
            agent any
            steps {
                script {
                    // Cache NuGet packages (this could be more complex if you want a more permanent cache)
                    if (isUnix()) {
                        sh 'dotnet restore'
                    } else {
                        bat 'dotnet restore'
                    }
                }
            }
        }

        stage('Restore Application') {
            matrix {
                axes {
                    axis {
                        name 'CONFIGURATION'
                        values 'Debug', 'Release'  // matrix configuration
                    }
                    axis {
                        name 'OS'
                        values 'ubuntu', 'windows', 'macos'  // specify the OS for matrix execution
                    }
                }
                agent {
                    label 'your-agent-label'  // Jenkins agent can be set here if needed
                }
                stages {
                    stage('Build & Restore') {
                        steps {
                            script {
                                if (env.OS == 'ubuntu' || env.OS == 'macos') {
                                    sh "dotnet restore ${env.SOLUTION_NAME} --configuration ${env.CONFIGURATION}"
                                } else {
                                    bat "msbuild ${env.SOLUTION_NAME} /t:Restore /p:Configuration=${env.CONFIGURATION}"
                                }
                            }
                        }
                    }
                }
            }
        }

        stage('Execute Unit Tests') {
            matrix {
                axes {
                    axis {
                        name 'CONFIGURATION'
                        values 'Debug', 'Release'
                    }
                    axis {
                        name 'OS'
                        values 'ubuntu', 'windows', 'macos'
                    }
                }
                agent any
                stages {
                    stage('Test') {
                        steps {
                            script {
                                if (env.OS == 'ubuntu' || env.OS == 'macos') {
                                    sh "dotnet test --no-restore --configuration ${env.CONFIGURATION}"
                                } else {
                                    bat "dotnet test --no-restore --configuration ${env.CONFIGURATION}"
                                }
                            }
                        }
                    }
                }
            }
        }

        stage('Packaging & Deployment') {
            when {
                expression {
                    // Only execute this for Windows, as per your original pipeline logic
                    return env.OS == 'windows'
                }
            }
            steps {
                script {
                    // Windows-specific logic for packaging & deployment
                    echo 'Packaging & Deployment for Windows...'
                    // Add additional Windows-specific deployment steps here
                }
            }
        }
    }

    post {
        always {
            echo 'Cleaning up resources...'
        }
        success {
            echo 'Build and tests passed!'
        }
        failure {
            echo 'Build failed. Please check the logs.'
        }
    }
}
