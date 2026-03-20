<?php
/**
 * GET /api/config - 返回配置
 */

echo json_encode([
    'ok'   => true,
    'data' => [
        'version' => '1.0',
    ],
]);
