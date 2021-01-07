# MixedReality-WebRTC-ayame

MixedReality-WebRTC-ayame は、MixedReality-WebRTC にてシグナリングサーバとして Ayame を利用するためのコードです。

## MixedReality-WebRTC とは

Microsoft が開発している、Mixed Reality アプリケーションで WebRTC を利用するためのライブラリです。

https://github.com/microsoft/MixedReality-WebRTC

## Ayame とは

時雨堂さんが開発している OSS の WebRTC 向けシグナリングサーバです。

https://github.com/OpenAyame/ayame

Ayame Labo という無料で利用できるサービスも公開されており、
これを利用すれば自前でサーバーを立てる必要がありません。

https://ayame-labo.shiguredo.jp

## 使い方

### プロジェクトへの導入 (Unity 2019 以降)

Project の Packages\manifest.json に以下の内容を追記してください。  
バージョン番号については、各リポジトリのリリースをご確認ください。

- https://github.com/microsoft/MixedReality-WebRTC/releases
- https://github.com/tarukosu/MixedReality-WebRTC-ayame/releases


```
{
  "scopedRegistries": [
    {
      "name": "Microsoft Mixed Reality",
      "url": "https://pkgs.dev.azure.com/aipmr/MixedReality-Unity-Packages/_packaging/Unity-packages/npm/registry/",
      "scopes": ["com.microsoft.mixedreality"]
    }
  ],
  "dependencies": {
    "com.microsoft.mixedreality.webrtc": "<バージョン番号>",
    "com.microsoft.mixedreality.webrtc.samples": "<バージョン番号>",
    "com.tarukosu.mixedreality.webrtc.ayame": "https://github.com/tarukosu/MixedReality-WebRTC-ayame.git?path=Assets/Microsoft.MixedReality.WebRTC.Unity.ThirdParty/Ayame#<バージョン番号>",
    ...
```

### Sample フォルダのインポート
Window > Package Manager から MixedReality-WebRTC Ayame を開き、Samples の Import ボタンを押してください。
