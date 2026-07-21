pipeline {
    agent any

    environment {
        DOTNET_CLI_TELEMETRY_OPTOUT = '1'
        CI = 'true'
    }

    options {
        timestamps()
        disableConcurrentBuilds()
        buildDiscarder(logRotator(numToKeepStr: '20'))
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Backend Build') {
            steps {
                sh 'dotnet restore JiraTrack.sln || dotnet restore API/JiraTrack.csproj Tests/JiraTrack.Tests.csproj'
                sh 'dotnet build Tests/JiraTrack.Tests.csproj -c Release --no-restore'
            }
        }

        stage('Backend Test') {
            steps {
                sh 'dotnet test Tests/JiraTrack.Tests.csproj -c Release --no-build --verbosity normal --collect:"XPlat Code Coverage"'
            }
        }

        stage('Frontend Build') {
            steps {
                dir('APP') {
                    sh 'npm ci'
                    sh 'npm run build -- --configuration=production'
                }
            }
        }

        stage('SonarQube') {
            when {
                expression { return env.SONAR_TOKEN?.trim() }
            }
            steps {
                echo 'SonarQube analysis skipped unless SONAR_TOKEN is configured on the Jenkins controller.'
            }
        }

        stage('Deploy Dev') {
            when {
                branch 'develop'
            }
            steps {
                echo 'Deploy to Dev — run docker compose or publish artifacts from this stage.'
            }
        }

        stage('Deploy Staging') {
            when {
                branch 'staging'
            }
            steps {
                echo 'Deploy to Staging environment.'
            }
        }

        stage('Deploy Prod') {
            when {
                branch 'main'
            }
            steps {
                echo 'Deploy to Production environment (manual approval recommended).'
            }
        }
    }

    post {
        always {
            junit allowEmptyResults: true, testResults: 'Tests/TestResults/*.xml'
            archiveArtifacts artifacts: 'APP/dist/**,API/bin/Release/**', allowEmptyArchive: true
        }
        success {
            echo 'JiraTrack pipeline completed successfully.'
        }
        failure {
            echo 'JiraTrack pipeline failed — check stage logs.'
        }
    }
}
