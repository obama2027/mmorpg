<?php
/**
 * GET /api/getVersionJson?platform=Android&version=1.0.2
 * Returns version.json content for Res/<platform>/<version>/version.json
 */

function isValidPlatform($platform) {
    return preg_match('/^[A-Za-z0-9_]+$/', $platform) === 1;
}

function isValidVersion($version) {
    return preg_match('/^[A-Za-z0-9_.-]+$/', $version) === 1;
}

$platform = $_GET['platform'] ?? '';
$version = $_GET['version'] ?? '';

if (!isValidPlatform($platform) || !isValidVersion($version)) {
    http_response_code(400);
    echo json_encode(['ok' => false, 'error' => 'Invalid platform or version']);
    exit;
}

$resRoot = dirname(__DIR__) . '/Res';
$versionJsonPath = $resRoot . '/' . $platform . '/' . $version . '/version.json';

if (!file_exists($versionJsonPath)) {
    http_response_code(404);
    echo json_encode(['ok' => false, 'error' => 'version.json not found']);
    exit;
}

header('Content-Type: application/json; charset=utf-8');
header('Access-Control-Allow-Origin: *');
echo file_get_contents($versionJsonPath);
