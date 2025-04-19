Summary: rfpproxy
Name: rfpproxy
Version: 0.0.2
Release: 3
License: MIT
URL: https://github.com/eventphone/rfpproxy
Requires: dotnet-runtime-8.0

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
%attr(755, root, root) %caps(cap_net_admin=+eip) /opt/rfpproxy/rfpproxy
%attr(755, root, root) /opt/rfpproxy/avm
%attr(755, root, root) /opt/rfpproxy/busyled
%attr(755, root, root) /opt/rfpproxy/compressipui
%attr(755, root, root) /opt/rfpproxy/mediatone
%attr(755, root, root) /opt/rfpproxy/morseled
%attr(755, root, root) /opt/rfpproxy/rfpproxyinject
%attr(755, root, root) /opt/rfpproxy/RfpProxy.ChangeLed
%attr(755, root, root) /opt/rfpproxy/rfpproxylog
%attr(755, root, root) /opt/rfpproxy/rfpproxydump
%attr(755, root, root) /opt/rfpproxy/rfpproxytraffic
%attr(755, root, root) /opt/rfpproxy/virtualrfp
%attr(644, root, root) /opt/rfpproxy/*.dll
/opt/rfpproxy/*.deps.json
/opt/rfpproxy/*.runtimeconfig.json
/lib/systemd/system/rfpproxy.service

%clean
mkdir -p rpm
mv %{_rpmdir}/noarch/* rpm/
rm -rf $RPM_BUILD_ROOT/

%pre
getent passwd rfpproxy >/dev/null 2>&1 || useradd -r -M -d /opt/rfpproxy rfpproxy

%post
if [ $1 -eq 1 ] ; then
  # Initial installation
  systemctl preset rfpproxy.service >/dev/null 2>&1 || :
fi

%preun
if [ $1 -eq 0 ] ; then
  # Package removal, not upgrade
  systemctl --no-reload disable rfpproxy.service > /dev/null 2>&1 || :
  systemctl stop rfpproxy.service > /dev/null 2>&1 || :
fi

%postun
systemctl daemon-reload >/dev/null 2>&1 || :
if [ $1 -ge 1 ] ; then
  # Package upgrade, not uninstall
  systemctl try-restart rfpproxy.service >/dev/null 2>&1 || :
fi
