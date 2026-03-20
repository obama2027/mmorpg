<?php
/**
 * 公共初始化：CORS、JSON 头、错误处理
 */

header('Content-Type: application/json; charset=utf-8');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: GET, POST, OPTIONS');
header('Access-Control-Allow-Headers: Content-Type');

if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') {
    http_response_code(204);
    exit;
}

set_exception_handler(function (Throwable $e) {
    http_response_code(500);
    echo json_encode(['ok' => false, 'error' => $e->getMessage()]);
});
