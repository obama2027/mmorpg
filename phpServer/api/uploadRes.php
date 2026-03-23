<?php
/**
 * POST /api/uploadRes?platform=Android
 * Request body: application/zip (binary zip)
 * Zip content should be the files under StreamingAssets/<Platform>:
 *   - version.json at zip root
 *   - launch.dll.bytes, game.dll.bytes, *.bundle, etc
 */

function isValidPlatform($platform) {
    return preg_match('/^[A-Za-z0-9_]+$/', $platform) === 1;
}

function rrmdir($dir) {
    if (!is_dir($dir)) return;
    $items = new RecursiveIteratorIterator(
        new RecursiveDirectoryIterator($dir, RecursiveDirectoryIterator::SKIP_DOTS),
        RecursiveIteratorIterator::CHILD_FIRST
    );
    foreach ($items as $item) {
        if ($item->isDir()) {
            @rmdir($item->getPathname());
        } else {
            @unlink($item->getPathname());
        }
    }
    @rmdir($dir);
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

function locateFileInDir($dir, $fileName) {
    if (!is_dir($dir)) return null;
    $it = new RecursiveIteratorIterator(
        new RecursiveDirectoryIterator($dir, RecursiveDirectoryIterator::SKIP_DOTS),
        RecursiveIteratorIterator::SELF_FIRST
    );
    foreach ($it as $item) {
        if ($item->isFile() && $item->getFilename() === $fileName) {
            return $item->getPathname();
        }
    }
    return null;
}

function rcopy($srcDir, $dstDir) {
    if (!is_dir($srcDir)) return;
    if (!is_dir($dstDir)) mkdir($dstDir, 0777, true);

    $it = new RecursiveIteratorIterator(
        new RecursiveDirectoryIterator($srcDir, RecursiveDirectoryIterator::SKIP_DOTS),
        RecursiveIteratorIterator::SELF_FIRST
    );

    foreach ($it as $item) {
        /** @var SplFileInfo $item */
        $rel = substr($item->getPathname(), strlen($srcDir) + 1);
        $target = $dstDir . DIRECTORY_SEPARATOR . $rel;

        if ($item->isDir()) {
            if (!is_dir($target)) mkdir($target, 0777, true);
        } else {
            $td = dirname($target);
            if (!is_dir($td)) mkdir($td, 0777, true);
            copy($item->getPathname(), $target);
        }
    }
}

$platform = $_GET['platform'] ?? '';
if (!isValidPlatform($platform)) {
    http_response_code(400);
    echo json_encode(['ok' => false, 'error' => 'Invalid platform']);
    exit;
}

$rawZip = file_get_contents('php://input');
if ($rawZip === false || $rawZip === '') {
    http_response_code(400);
    echo json_encode(['ok' => false, 'error' => 'Empty zip body']);
    exit;
}

$resRoot = dirname(__DIR__) . '/Res';
$platformDir = $resRoot . DIRECTORY_SEPARATOR . $platform;
$versionTxtPath = $platformDir . DIRECTORY_SEPARATOR . 'version.txt';

$tmpBase = sys_get_temp_dir() . DIRECTORY_SEPARATOR . 'uploadRes_' . uniqid();
$extractDir = $tmpBase . DIRECTORY_SEPARATOR . 'extract';
$zipPath = $tmpBase . DIRECTORY_SEPARATOR . 'upload.zip';

@mkdir($extractDir, 0777, true);
@mkdir($tmpBase, 0777, true);
file_put_contents($zipPath, $rawZip);

$extracted = false;
if (class_exists('ZipArchive')) {
    $zip = new ZipArchive();
    if ($zip->open($zipPath) === true) {
        $zip->extractTo($extractDir);
        $zip->close();
        $extracted = true;
    }
}
if (!$extracted && class_exists('PharData')) {
    try {
        $phar = new PharData($zipPath, 0, null, Phar::ZIP);
        $phar->extractTo($extractDir);
        $extracted = true;
    } catch (Exception $e) {
        // fall through
    }
}
if (!$extracted) {
    rrmdir($tmpBase);
    http_response_code(500);
    echo json_encode([
        'ok' => false,
        'error' => 'Zip extraction failed. Enable PHP zip extension (extension=zip in php.ini) or phar extension.',
    ]);
    exit;
}

$uploadedVersionJsonPath = locateFileInDir($extractDir, 'version.json');
if ($uploadedVersionJsonPath === null) {
    rrmdir($tmpBase);
    http_response_code(400);
    echo json_encode(['ok' => false, 'error' => 'version.json not found in zip']);
    exit;
}

$jsonRaw = file_get_contents($uploadedVersionJsonPath);
$jsonRaw = preg_replace('/^\xEF\xBB\xBF/', '', $jsonRaw);
$uploadedData = json_decode($jsonRaw, true);
$versionValue = isset($uploadedData['Version']) ? trim((string)$uploadedData['Version']) : (isset($uploadedData['version']) ? trim((string)$uploadedData['version']) : '');
if (!is_array($uploadedData) || $versionValue === '') {
    rrmdir($tmpBase);
    http_response_code(400);
    $err = json_last_error_msg();
    echo json_encode(['ok' => false, 'error' => 'Invalid version.json in zip. ' . $err . ' Version missing or empty.']);
    exit;
}

// Determine target version by server version.txt.
if (!file_exists($versionTxtPath)) {
    $targetVersion = $versionValue !== '' ? $versionValue : "1.0.0";
} else {
    $serverVersion = trim((string)file_get_contents($versionTxtPath));
    if ($serverVersion === '') $serverVersion = "1.0.0";
    $targetVersion = incrementVersionLastSegment($serverVersion);
}

// Update uploaded version.json Version -> targetVersion (case B).
$uploadedData['Version'] = $targetVersion;
$uploadedJson = json_encode($uploadedData, JSON_PRETTY_PRINT | JSON_UNESCAPED_SLASHES);
file_put_contents($uploadedVersionJsonPath, $uploadedJson);

// Prepare destination: Res/<platform>/<targetVersion>/
if (!is_dir($resRoot)) mkdir($resRoot, 0777, true);
if (!is_dir($platformDir)) mkdir($platformDir, 0777, true);

$targetDir = $platformDir . DIRECTORY_SEPARATOR . $targetVersion;
if (is_dir($targetDir)) {
    rrmdir($targetDir);
}
mkdir($targetDir, 0777, true);

// Copy all files first.
rcopy($extractDir, $targetDir);

// Then write version.txt (must be last).
file_put_contents($versionTxtPath, $targetVersion);

rrmdir($tmpBase);

echo json_encode([
    'ok' => true,
    'platform' => $platform,
    'version' => $targetVersion,
]);

