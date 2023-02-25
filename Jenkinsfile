def version = ''
def chartVersion = ''
def chartAction = ''
def patch = ''
def shouldUninstall = ''
def deployAction = ''
def message = ''
pipeline {
    agent any

    stages {
        stage('Prepare') {
            steps {
                sh 'echo "Build Summary: \n" > SUMMARY'
                script {
                    withCredentials([string(credentialsId: 'hangouts_token', variable: 'CHATS_TOKEN')]) {
                        hangoutsNotifyBuildStart token: "$CHATS_TOKEN",threadByJob: false
                    }
                    
                    // Get some code from a GitHub repository
                    git branch: '$BRANCH_NAME', url: 'https://github.com/Veronneau-Techno-Conseil/comax-reverseproxy.git'
                
                    version = readFile('VERSION').trim()
                    chartVersion = readFile('./helm/VERSION').trim()
                    patch = version
                }
            }
        }
        
        stage('Build') {
            steps {
                withCredentials([[$class: 'UsernamePasswordMultiBinding', credentialsId:'dockerhub_creds', usernameVariable: 'USERNAME', passwordVariable: 'PASSWORD']]) {
                    sh 'if [ -z "$(docker buildx ls | grep multiarch)" ]; then docker buildx create --name multiarch --driver docker-container --use; else docker buildx use multiarch; fi'
                    sh "docker login -u ${USERNAME} -p ${PASSWORD}"

                    sh "docker buildx build --push -t vertechcon/comax-reverseproxyui:latest -t vertechcon/comax-reverseproxyui:${patch} --platform linux/amd64,linux/arm64 -f rpui.Dockerfile ."

                    sh "docker buildx build --push -t vertechcon/comax-reverseproxyop:latest -t vertechcon/comax-reverseproxyop:${patch} --platform linux/arm64 -f operator.Dockerfile ."                    
                    sh "docker buildx build --push -t vertechcon/comax-reverseproxyop:latest -t vertechcon/comax-reverseproxyop:${patch} --platform linux/amd64 -f operator.Dockerfile ."                    
                }
            }

            post {
                success {
                    script {
                        currentBuild.displayName = version
                    }
                }                
            }
        }
        stage('Prep Helm Reverse Proxy') {
            agent {
                docker {
                    image 'python:3.9.16-bullseye'
                    reuseNode true
                }
            }
            steps {
                sh 'mkdir penv && python3 -m venv ./penv'
                sh '. penv/bin/activate && pwd && ls -l && pip install -r ./build/requirements.txt && python3 ./build/processchart.py'
                sh 'curl -k https://charts.vtck3s.lan/api/charts/comax-reverseproxy/${chartVersion} | jq \'.name | "DEPLOY"\' > CHART_ACTION'
                script {
                    chartAction = readFile('CHART_ACTION').replace('"','').trim()
                }
            }
        }
        stage('Helm Reverse Proxy') {
            when{
                expression {
                    return chartAction == "DEPLOY"
                }
            }
            steps {
                withCredentials([file(credentialsId: 'edgek3s', variable: 'kubecfg'), file(credentialsId: 'helmrepos', variable: 'repos')]) {
                    sh 'helm lint ./helm/'
                    sh 'helm repo update --repository-config ${repos}'
                    sh 'helm dependency update ./helm/ --repository-config ${repos}'
                    sh 'helm package ./helm/ --repository-config ${repos}'
                    sh 'CHARTVER=$(cat ./helm/VERSION) && curl -k --data-binary "@comax-reverseproxy-$CHARTVER.tgz" https://charts.vtck3s.lan/api/charts'
                }
            }
        }
        stage('SKIP Helm Reverse Proxy') {
            when{
                expression {
                    return chartAction != "DEPLOY"
                }
            }
            steps {
                sh 'echo "Skipped helm chart deployment du to preexisting chart version ${chartVersion} \n" >> SUMMARY'
            }
        }
        stage('Prepare Application deployment Reverse Proxy') {
            when{
                expression {
                    return env.BRANCH_NAME.startsWith('release')
                }
            }
            steps {
                withCredentials([file(credentialsId: 'edgek3s', variable: 'kubecfg'), file(credentialsId: 'helmrepos', variable: 'repos')]) {
                    sh 'helm repo update --repository-config ${repos}'
                    sh 'helm dependency update ./helm --repository-config ${repos}'
                    sh 'helm list -n comax-reverseproxy --output=json --kubeconfig $kubecfg > HELM_LIST'
                    sh 'cat HELM_LIST'
                    sh 'jq \'select(.[].name == "reverseproxy") | select(.[].status == "deployed") | "upgrade" \' HELM_LIST > DEPLOY_ACTION'
                    sh 'jq \'select(.[].name == "reverseproxy") | select(.[].status != "deployed") | "uninstall" \' HELM_LIST > SHOULD_UNINSTALL'
                    sh 'cat DEPLOY_ACTION && cat SHOULD_UNINSTALL'
                    script {
                        deployAction = readFile('DEPLOY_ACTION').replace('"','').trim()
                        shouldUninstall = readFile('SHOULD_UNINSTALL').replace('"','').trim()
                    }
                    echo "Deploy action: ${deployAction}"
                    echo "Should uninstall: ${shouldUninstall}"
                }
            }
        }
        stage('Uninstall Application deployment Reverse Proxy') {
            when{
                expression {
                    return env.BRANCH_NAME.startsWith('release') && shouldUninstall == 'uninstall'
                }
            }
            steps {
                withCredentials([file(credentialsId: 'edgek3s', variable: 'kubecfg'), file(credentialsId: 'helmrepos', variable: 'repos')]) {
                    sh 'helm -n comax-reverseproxy uninstall reverseproxy --kubeconfig ${kubecfg}'
                }
            }
        }
        stage('Install Application deployment Reverse Proxy') {
            when{
                expression {
                    return env.BRANCH_NAME.startsWith('release') && deployAction != "upgrade"
                }
            }
            steps {
                echo "Deploy action: ${deployAction}"
                withCredentials([file(credentialsId: 'edgek3s', variable: 'kubecfg'), file(credentialsId: 'helmrepos', variable: 'repos'), file(credentialsId: 'rpvalues', variable: 'rpvalues')]) {
                    sh 'helm -n comax-reverseproxy install reverseproxy ./helm/ --kubeconfig ${kubecfg} --repository-config ${repos} -f ${rpvalues}'
                }
            }
        }
        stage('Upgrade Application deployment Reverse Proxy') {
            when{
                expression {
                    return env.BRANCH_NAME.startsWith('release') && deployAction == "upgrade"
                }
            }
            steps {
                withCredentials([file(credentialsId: 'edgek3s', variable: 'kubecfg'), file(credentialsId: 'helmrepos', variable: 'repos'), file(credentialsId: 'rpvalues', variable: 'rpvalues')]) {
                    sh 'helm -n comax-reverseproxy upgrade reverseproxy ./helm/ --kubeconfig ${kubecfg} --repository-config ${repos} -f ${rpvalues}'
                }
            }
        }
                
        stage('Finalize') {
            steps {
                script {
                    message = readFile('SUMMARY')
                }
            }
        }
    }
    post {
        success {
            withCredentials([string(credentialsId: 'hangouts_token', variable: 'CHATS_TOKEN')]) {
                hangoutsNotify message: message, token: "$CHATS_TOKEN", threadByJob: false
                hangoutsNotifySuccess token: "$CHATS_TOKEN", threadByJob: false
            }
        }
        failure {
            withCredentials([string(credentialsId: 'hangouts_token', variable: 'CHATS_TOKEN')]) {
                hangoutsNotify message: message, token: "$CHATS_TOKEN", threadByJob: false
                hangoutsNotifyFailure token: "$CHATS_TOKEN", threadByJob: false
            }
        }
        always {
            script {
                cleanWs()
            }
        }
    }
}
