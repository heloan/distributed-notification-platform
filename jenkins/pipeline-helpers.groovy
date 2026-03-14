# =============================================================================
# Jenkins Shared Library Helpers
# =============================================================================
# Groovy helper functions that can be loaded from the Jenkinsfile.
# Usage in Jenkinsfile:
#   def helpers = load 'jenkins/pipeline-helpers.groovy'
#   helpers.notifySlack('success')
# =============================================================================

/**
 * Send a Slack notification with build status.
 */
def notifySlack(String status) {
    def color = [
        'success' : 'good',
        'failure' : 'danger',
        'unstable': 'warning'
    ][status] ?: '#808080'

    def emoji = [
        'success' : '✅',
        'failure' : '❌',
        'unstable': '⚠️'
    ][status] ?: 'ℹ️'

    def message = "${emoji} *DSNP Build #${env.BUILD_NUMBER}* — ${status.toUpperCase()}\n" +
                  "Branch: `${env.BRANCH_NAME}`\n" +
                  "Commit: `${env.GIT_COMMIT?.take(7)}`\n" +
                  "<${env.BUILD_URL}|View Build>"

    // Uncomment when Slack plugin is configured:
    // slackSend(channel: '#ci-cd', color: color, message: message)
    echo message
}

/**
 * Get the semantic version tag for the current build.
 * On main → vX.Y.Z based on git tags
 * On develop → vX.Y.Z-rc.BUILD_NUMBER
 * On feature → vX.Y.Z-BRANCH.BUILD_NUMBER
 */
def getVersionTag() {
    def baseVersion = sh(script: "git describe --tags --abbrev=0 2>/dev/null || echo 'v0.1.0'", returnStdout: true).trim()

    switch (env.BRANCH_NAME) {
        case 'main':
            return baseVersion
        case 'develop':
            return "${baseVersion}-rc.${env.BUILD_NUMBER}"
        default:
            def safeBranch = env.BRANCH_NAME.replaceAll('[^a-zA-Z0-9]', '-').take(20)
            return "${baseVersion}-${safeBranch}.${env.BUILD_NUMBER}"
    }
}

/**
 * Wait for a service to become healthy via HTTP health check.
 */
def waitForService(String url, int maxRetries = 30, int intervalSec = 5) {
    echo "Waiting for ${url} to be healthy..."
    for (int i = 0; i < maxRetries; i++) {
        def status = sh(script: "curl -sf ${url} > /dev/null 2>&1 && echo 'UP' || echo 'DOWN'", returnStdout: true).trim()
        if (status == 'UP') {
            echo "${url} is healthy ✅"
            return true
        }
        echo "  Attempt ${i + 1}/${maxRetries} — waiting ${intervalSec}s..."
        sleep intervalSec
    }
    error "${url} did not become healthy after ${maxRetries * intervalSec}s"
}

/**
 * Check Docker image size against threshold.
 */
def checkImageSize(String imageName, int maxSizeMB) {
    def sizeBytes = sh(script: "docker image inspect ${imageName} --format='{{.Size}}'", returnStdout: true).trim()
    def sizeMB = (sizeBytes as Long) / (1024 * 1024)
    echo "Image ${imageName}: ${sizeMB.round(1)} MB (limit: ${maxSizeMB} MB)"
    if (sizeMB > maxSizeMB) {
        error "Image ${imageName} exceeds size limit: ${sizeMB.round(1)} MB > ${maxSizeMB} MB"
    }
}

return this
