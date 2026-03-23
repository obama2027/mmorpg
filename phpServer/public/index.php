<?php
/**
 * 统一入口：解析路径并路由到 api/*.php
 */

require_once dirname(__DIR__) . '/bootstrap.php';

$uri = $_SERVER['REQUEST_URI'] ?? '/';
$path = parse_url($uri, PHP_URL_PATH);
$path = rtrim($path, '/') ?: '/';
$method = $_SERVER['REQUEST_METHOD'] ?? 'GET';

$routes = [
    'GET' => [
        '/api/config' => 'config.php',
        '/api/echo'   => 'echo.php',
        '/api/getNextResVersion' => 'getNextResVersion.php',
        '/api/getResVersion' => 'getResVersion.php',
        '/api/getVersionJson' => 'getVersionJson.php',
        '/api/downloadResFile' => 'downloadResFile.php',
    ],
    'POST' => [
        '/api/echo'  => 'echo.php',
        '/api/login' => 'login.php',
        '/api/uploadRes' => 'uploadRes.php',
    ],
];

$handler = $routes[$method][$path] ?? null;

if ($handler) {
    $file = dirname(__DIR__) . '/api/' . $handler;
    if (is_file($file)) {
        require $file;
        exit;
    }
}

http_response_code(404);
echo json_encode(['ok' => false, 'error' => 'Not Found', 'path' => $path]);
