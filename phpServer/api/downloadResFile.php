<?php
/**
 * GET /api/downloadResFile?platform=Android&version=1.0.2&file=base.dll.bytes
 * Streams the requested file from Res/<platform>/<version>/
 */

function isValidPlatform($platform) {
    return preg_match('/^[A-Za-z0-9_]+$/', $platform) === 1;
}

function isValidVersion($version) {
    return preg_match('/^[A-Za-z0-9_.-]+$/', $version) === 1;
}

function isValidFileName($file) {
    return preg_match('/^[A-Za-z0-9_.-]+$/', $file) === 1 && strpos($file, '..') === false;
}

$platform = $_GET['platform'] ?? '';
$version = $_GET['version'] ?? '';
$file = $_GET['file'] ?? '';

if (!isValidPlatform($platform) || !isValidVersion($version) || !isValidFileName($file)) {
    http_response_code(400);
    header('Content-Type: application/json; charset=utf-8');
    echo json_encode(['ok' => false, 'error' => 'Invalid parameters']);
    exit;
}

$resRoot = dirname(__DIR__) . '/Res';
$filePath = realpath($resRoot . '/' . $platform . '/' . $version . '/' . $file);

if ($filePath === false || !file_exists($filePath)) {
    http_response_code(404);
    header('Content-Type: application/json; charset=utf-8');
    echo json_encode(['ok' => false, 'error' => 'File not found']);
    exit;
}

$baseDir = realpath($resRoot . '/' . $platform . '/' . $version);
if ($baseDir === false || strpos($filePath, $baseDir) !== 0) {
    http_response_code(403);
    header('Content-Type: application/json; charset=utf-8');
    echo json_encode(['ok' => false, 'error' => 'Access denied']);
    exit;
}

header('Content-Type: application/octet-stream');
header('Content-Length: ' . filesize($filePath));
header('Access-Control-Allow-Origin: *');
header('Content-Disposition: attachment; filename="' . basename($file) . '"');
readfile($filePath);
