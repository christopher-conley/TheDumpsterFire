FROM golang:alpine AS build

WORKDIR /tmp/src
RUN mkdir /artifacts \
    && apk update \
    && apk upgrade \
    && apk add git procps \
    && git clone https://github.com/xxxserxxx/gotop.git /tmp/src \
    && sed -i 's|"ps", "-axo"|"/bin/ps", "axo"|g' /tmp/src/widgets/proc_linux.go
# The distroless debian image doesn't contain 'ps', and gotop needs it to display processes
# Alpine 'ps' won't work with "-axo" the way gotop is calling it, needs "axo" instead

RUN VERS=$(git tag -l --sort=-v:refname | sed 's/v\([^-].*\)/\1/g' | head -1 | tr -d '-' ).$(git describe --long --tags | sed 's/\([^-].*\)-\([0-9]*\)-\(g.*\)/r\2.\3/g' | tr -d '-') DAT=$(date +%Y%m%dT%H%M%S) CGO_ENABLED=0 go build -o gotop -ldflags "-s -w -X main.Version=v${VERS} -X main.BuildDate=${DAT}" ./cmd/gotop \
    && cp "/tmp/src/gotop" "/artifacts/gotop"
COPY ./bootstrap.sh /artifacts/bootstrap.sh

FROM gcr.io/distroless/static-debian12
COPY --from=build /artifacts/gotop /usr/bin/gotop
COPY --from=build /artifacts/bootstrap.sh /tmp/bootstrap.sh
COPY --from=build /bin/ps /bin/ps
COPY --from=build /bin/busybox /tmp/busybox

# Libraries required by the busybox & ps binaries
COPY --from=build /lib/ld-musl-x86_64.so.1 /lib/ld-musl-x86_64.so.1
COPY --from=build /usr/lib/libproc2.so.0.0.2 /usr/lib/libproc2.so.0.0.2
COPY --from=build /usr/lib/libintl.so.8.4.0 /usr/lib/libintl.so.8.4.0

# /bin/sh doesn't exist in distroless, so this is necessary to bootstrap
SHELL ["/tmp/busybox", "sh", "-c"]

RUN /tmp/busybox sh /tmp/bootstrap.sh \
    && rm /tmp/bootstrap.sh

WORKDIR /home/gotop
SHELL ["/bin/sh", "-c"]

USER gotop
RUN /usr/bin/gotop --write-config

CMD ["/usr/bin/gotop", "-C", "/home/gotop/.config/gotop/gotop.conf"]
