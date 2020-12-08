Summary: rfpproxy
Name: rfpproxy
Version: 0.0.1
Release: 1
License: MIT
URL: https://github.com/eventphone/rfpproxy
Requires: dotnet-runtime-3.0

%global __requires_exclude_from ^.*$
%global __provides_exclude_from ^.*$

%description
rfpproxy

%prep
mkdir -p $RPM_BUILD_ROOT/opt/rfpproxy $RPM_BUILD_ROOT/lib/systemd/system

cp -ru publish/*/* $RPM_BUILD_ROOT/opt/rfpproxy
rm $RPM_BUILD_ROOT/opt/rfpproxy/*.pdb
cp rfpproxy.service $RPM_BUILD_ROOT/lib/systemd/system/.
exit

%files
%caps(cap_net_admin=+eip) /opt/rfpproxy/rfpproxy
/opt/rfpproxy/avm
/opt/rfpproxy/busyled
/opt/rfpproxy/compressipui
/opt/rfpproxy/mediatone
/opt/rfpproxy/morseled
/opt/rfpproxy/rfpproxyinject
/opt/rfpproxy/RfpProxy.ChangeLed
/opt/rfpproxy/rfpproxylog
/opt/rfpproxy/rfpproxydump
/opt/rfpproxy/rfpproxytraffic
%attr(644, root, root) /opt/rfpproxy/*.dll
/opt/rfpproxy/*.deps.json
/opt/rfpproxy/*.runtimeconfig.json
/lib/systemd/system/rfpproxy.service

%clean
mkdir rpm
mv %{_rpmdir}/noarch/* rpm/
rm -rf $RPM_BUILD_ROOT/

%pre
getent passwd rfpproxy >/dev/null 2>&1 || useradd -r -M -d /opt/rfpproxy rfpproxy

%post
%systemd_post rfpproxy.service

%preun
%systemd_preun rfpproxy.service

%postun
%systemd_postun_with_restart rfpproxy.service