<?php
/**
 * POST /api/login - 简易登录（不验证，直接放行）
 */

$raw = file_get_contents('php://input');
$body = $raw ? (json_decode($raw, true) ?? []) : $_POST;

$username = $body['username'] ?? '';
$password = $body['password'] ?? '';

echo json_encode([
    'ok'    => true,
    'token' => 'mock_token',
    'user'  => [
        'username' => $username ?: 'guest',
    ],
]);
