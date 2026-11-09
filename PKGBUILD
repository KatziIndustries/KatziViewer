pkgname=KatziViewer-git
pkgver=2
pkgdesc="A simple application for viewing images"
arch=('x86_64')
url="https://github.com/KatziIndustries/KatziViewer"
license=('MIT')
depends=('sdl3' 'sdl3_image' 'sdl3_ttf')
makedepends=('dotnet-sdk' 'git')
source=("git+$url.git")
sha256sums=('SKIP')

build() {
    cd "$srcdir/KatziViewer"

    dotnet restore --locked-mode
    dotnet publish -c Release
}

package() {
    install -Dm755 "$srcdir/KatziViewer/publish/KatziViewer" \
        "$pkgdir/usr/bin/KatziViewer"

    install -Dm644 "$srcdir/KatziViewer/LICENSE" \
        "$pkgdir/usr/share/licenses/$pkgname/LICENSE"

    install -Dm644 "$srcdir/KatziViewer/KatziViewer.desktop" \
            "$pkgdir/usr/share/applications/KatziViewer.desktop"

    install -Dm644 "$srcdir/KatziViewer/KatziViewer.png" \
            "$pkgdir/usr/share/icons/hicolor/256x256/apps/KatziViewer.png"
}
