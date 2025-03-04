copy /Y "./manifest.json" "./release"
copy /Y "./icon.png" "./release"
copy /Y "./README.md" "./release"
copy /Y ".\\bin\\Debug\\net462\\RuneTablet.dll" "./release/RuneTablet/"
xcopy /S /Y "./Assets" "./release/RuneTablet/Assets/"

