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
                    echo "Installing .NET Core SDK for Ubuntu..."

                    // Install .NET Core SDK
                    if (isUnix()) {
                        sh '''
                            # Add Microsoft package source and install .NET SDK
                            sudo apt-get update
                            sudo apt-get install -y wget apt-transport-https software-properties-common
                            wget https://packages.microsoft.com/config/ubuntu/20.04/prod.list
                            sudo mv prod.list /etc/apt/sources.list.d/microsoft-prod.list
                            sudo apt-get update
                            sudo apt-get install -y dotnet-sdk-8.0
                        '''
                    } else {
                        bat '''
                            choco install dotnetcore-sdk --version 8.0
                        '''
                    }
                }
            }
        }

        stage('Cache NuGet Packages') {
            agent any
            steps {
                script {
                    echo "Caching NuGet Packages..."
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
                                echo "Restoring .NET application..."
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
                                echo "Running Unit Tests..."
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
