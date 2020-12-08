Summary: rfpproxy
Name: rfpproxy
Version: 0.0.1
Release: 1
License: MIT
URL: https://github.com/eventphone/rfpproxy
Requires: dotnet-runtime-3.1

%description
rfpproxy

%prep
mkdir -p $RPM_BUILD_ROOT/opt/rfpproxy $RPM_BUILD_ROOT/lib/systemd/system

cp -r ../publish/* $RPM_BUILD_ROOT/opt/rfpproxy
cp ../rfpproxy.service $RPM_BUILD_ROOT/lib/systemd/system/.
exit

%files
/opt/rfpproxy/*/*
/lib/systemd/system/rfpproxy.service

%clean
mv %{_rpmdir}/noarch/* ../publish/
rm -rf $RPM_BUILD_ROOT/

%post
%systemd_post rfpproxy.service

%preun
%systemd_preun rfpproxy.service

%postun
%systemd_postun_with_restart rfpproxy.service