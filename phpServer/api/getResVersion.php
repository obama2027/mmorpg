<?php
/**
 * GET /api/getResVersion?platform=Android
 * Returns current version from Res/<platform>/version.txt
 */

function isValidPlatform($platform) {
    return preg_match('/^[A-Za-z0-9_]+$/', $platform) === 1;
}

$platform = $_GET['platform'] ?? '';
if (!isValidPlatform($platform)) {
    http_response_code(400);
    echo json_encode(['ok' => false, 'error' => 'Invalid platform']);
    exit;
}

$resRoot = dirname(__DIR__) . '/Res';
$versionTxtPath = $resRoot . '/' . $platform . '/version.txt';

if (!file_exists($versionTxtPath)) {
    echo json_encode([
        'ok' => true,
        'platform' => $platform,
        'version' => '',
    ]);
    exit;
}

$version = trim((string)file_get_contents($versionTxtPath));

echo json_encode([
    'ok' => true,
    'platform' => $platform,
    'version' => $version,
]);
