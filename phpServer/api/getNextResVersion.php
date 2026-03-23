<?php
/**
 * GET /api/getNextResVersion?platform=Android
 * - If Res/<platform>/version.txt doesn't exist: hasVersionTxt=false
 * - If exists: return nextVersion = last segment + 1
 */

function isValidPlatform($platform) {
    return preg_match('/^[A-Za-z0-9_]+$/', $platform) === 1;
}

function incrementVersionLastSegment($version) {
    $version = trim((string)$version);
    if ($version === '') return "1.0.0";

    $parts = explode('.', $version);
    $lastIndex = count($parts) - 1;
    $last = intval($parts[$lastIndex] ?? 0);
    $parts[$lastIndex] = (string)($last + 1);
    return implode('.', $parts);
}

$platform = $_GET['platform'] ?? '';
if (!isValidPlatform($platform)) {
    http_response_code(400);
    echo json_encode(['ok' => false, 'error' => 'Invalid platform']);
    exit;
}

$resRoot = dirname(__DIR__) . '/Res';
$platformDir = $resRoot . DIRECTORY_SEPARATOR . $platform;
$versionTxtPath = $platformDir . DIRECTORY_SEPARATOR . 'version.txt';

if (!file_exists($versionTxtPath)) {
    echo json_encode([
        'ok' => true,
        'platform' => $platform,
        'hasVersionTxt' => false,
        'currentVersion' => '',
        'nextVersion' => '',
    ]);
    exit;
}

$serverVersion = trim((string)file_get_contents($versionTxtPath));
if ($serverVersion === '') {
    $serverVersion = "1.0.0";
}
$nextVersion = incrementVersionLastSegment($serverVersion);

echo json_encode([
    'ok' => true,
    'platform' => $platform,
    'hasVersionTxt' => true,
    'currentVersion' => $serverVersion,
    'nextVersion' => $nextVersion,
]);

