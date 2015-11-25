
# LIBSTEST=-reference:NUnit.Framework,Microsoft.CSharp,PromiseFuture
MONOPATH=/Library/Frameworks/Mono.framework/Libraries/mono/4.5/
NUNITCONSOLE="/Library/Frameworks/Mono.framework/Versions/Current/bin/nunit-console4"

XAMARINDIR="/Applications/Xamarin Studio.app"
MDTOOL=$(XAMARINDIR)/Contents/MacOS/mdtool

#clean:
#	find . -name "*.dll" -delete


build:
	$(MDTOOL) build

test: build
	MONO_PATH=$(MONOPATH) $(NUNITCONSOLE) Tests/Tests.csproj --nologo
	
