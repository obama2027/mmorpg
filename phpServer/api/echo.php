<?php
/**
 * GET|POST /api/echo - 回显参数，用于测试
 */

$result = ['ok' => true];

if ($_SERVER['REQUEST_METHOD'] === 'GET') {
    $result['get'] = $_GET;
}

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $result['post'] = $_POST;
    $raw = file_get_contents('php://input');
    if ($raw !== '') {
        $decoded = json_decode($raw, true);
        $result['body'] = $decoded ?? $raw;
    }
}

echo json_encode($result);
