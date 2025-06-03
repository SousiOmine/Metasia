このリポジトリには、マルチプラットフォーム対応動画編集ソフト"Metasia"のソースコードが格納されています。
Metasiaは主にC#とAvaloniaによって構築されています。

# リポジトリの構造
`Metasia.Core`には、動画編集ソフトとしての基幹的な処理が含まれています。Core部分はGUIに依存してはいけません。
`Metasia.Editor`には、Avaloniaによって構築されるGUI部分のコードや、プロジェクトの編集や保存といった、GUIに深く依存する処理が含まれています。
Editor部分のコードではMVVMパターンを採用しています。ModelはViewModelに、ViewModelはViewに依存してはいけません。
`Test`には、Metasiaのコード品質を担保するためのテストコードが含まれています。

# ビルド方法
`dotnet build Metasia.Core`や`dotnet build Metasia.Editor`を実行することで、それぞれのプロジェクトをビルドすることができます。
