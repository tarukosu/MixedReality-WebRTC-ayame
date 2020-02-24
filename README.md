# MixedReality-WebRTC-ayame
このリポジトリを使うことで、MixedReality-WebRTC にてシグナリングサーバとして Ayame を利用できます。

# MixedReality-WebRTC とは
Microsoft が開発している、Mixed Reality アプリケーションで WebRTC を利用するためのライブラリです。

https://github.com/microsoft/MixedReality-WebRTC

# Ayame とは
時雨堂さんが開発している OSS の WebRTC 向けシグナリングサーバです。

https://github.com/OpenAyame/ayame

Ayame Lite という無料で利用できるサービスも公開されており、
これを利用すれば自前でサーバーを立てる必要がありません。

https://ayame-lite.shiguredo.jp/beta

# セットアップ
1. MixedReality-WebRTC の Unity プロジェクトをセットアップ
1. MixedReality-WebRTC-ayame をインポート
1. 以下の DLL をインポート
    - WebSocket4Net
    - Newtonsoft.Json

# 使い方
MixedReality-WebRTC のサンプルで用いられている `NodeDssSignaler` の代わりに、`AyameSignaler` をお使いください。

サンプルシーン: `VideoChatAyameDemo.scene`
