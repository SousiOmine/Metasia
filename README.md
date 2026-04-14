# Metasia

Metasiaは、無料でオープンソースな動画編集ソフトです。

> [!IMPORTANT]
> 現在のMetasiaエディタのビルドには、動画や音声といったメディアファイルの読み込みおよび出力に必要な実装が含まれていません。  
> これらの機能を利用するために、別途プラグインを導入する必要があります。

### 主な特徴
- マルチプラットフォーム   
  Windows, macOS, Debian系Linuxで動作します。
- 高度なアニメーション機能  
  中間点毎に設定可能な移動ロジックにより、思うがままにアニメーションを表現できます。
- プラグインによる機能拡張  
  プラグインからクリップやエフェクト、ユーザーインターフェースを追加できます。
- 開発はベータ段階のため、破壊的変更が行われる可能性があります。

### 動作環境
- Windows 11 x64系CPU搭載のPC  
  (arm系CPU搭載機でも互換モードで動くかもしれませんが未確認)
- macOS 15.0以上 Apple Silicon搭載モデル
- Debian系Linux x64系CPU搭載のPC RAM4GB以上

### 使用方法
1. [Releaseページ](https://github.com/SousiOmine/Metasia/releases)から最新のバイナリをダウンロードし、解凍してください。  
1. 解凍してできた全てのファイルをアプリケーションをインストールしたいフォルダにコピーしてください。
1. コピーしたフォルダのうち、Windowsであれば`Metasia.Editor.exe`、
macOSであれば`MetasiaEditor.app`、Linuxであれば`Metasia.Editor`を実行してください。

> [!WARNING]
> macOS向けバイナリは現在署名されずに配布されています。
> これは開発者に、署名に必要なApple Developer Programを登録する資金的余裕がないためです。  
> ウェブからダウンロードした未署名のAppを実行するには、ダウンロード時に付与される属性`com.apple.quarantine`を削除する必要があります。
> `MetasiaEditor.app`と同じ階層のターミナルで`xattr -cr MetasiaEditor.app`を実行することでこの属性を削除することができますが、この行為によって発生した損害について、開発者は一切の責任を負いません。