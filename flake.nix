{
  description = "Modern .NET + Neovim dev shell with Copilot, Spec-JIT, and gh-dash";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs = { self, nixpkgs, flake-utils }:
    flake-utils.lib.eachDefaultSystem (system:
      let
        pkgs = import nixpkgs {
          inherit system;
          config.allowUnfree = true;
        };
        # Wrap Neovim with plugins so they are on runtimepath at startup
        neovim-with-plugins = pkgs.neovim.override {
          configure = {
            packages.myPlugins = with pkgs.vimPlugins; {
              start = [
                nvim-lspconfig
                nvim-cmp
                nvim-dap
                copilot-vim
              ];
              opt = [ ];
            };
            customRC = ''
              lua << EOF
              require('lspconfig').omnisharp.setup{}
              EOF
            '';
          };
        };
      in rec {
        devShells.default = pkgs.mkShell {
          name = "dotnet-neovim-agentic-shell";

          buildInputs = [
            pkgs.dotnet-sdk_8
            pkgs.omnisharp-roslyn
            pkgs.netcoredbg
            pkgs.nodejs_20
            neovim-with-plugins
            pkgs.git
            pkgs.gh
            pkgs.ripgrep
            pkgs.fd
          ];

          shellHook = ''
            export XDG_CONFIG_HOME=$(mktemp -d)
            export XDG_DATA_HOME=$(mktemp -d)
            export XDG_CACHE_HOME=$(mktemp -d)

            # Install GitHub CLI extensions (idempotent within this shell)
            if ! gh extension list 2>/dev/null | grep -q 'dlvhdr/gh-dash'; then
              gh extension install dlvhdr/gh-dash
            fi
            if ! gh extension list 2>/dev/null | grep -q 'github/spec-kit'; then
              gh extension install github/spec-kit
            fi

            if ! gh extension list 2>/dev/null | grep -q 'HaywardMorihara/gh-tidy'; then
              gh extension install HaywardMorihara/gh-tidy
            fi

            if ! gh extension list 2>/dev/null | grep -q 'antgrutta/gh-discussions'; then
              gh extension install antgrutta/gh-discussions
            fi

            echo "✅ Agentic .NET dev shell ready. Run 'nvim' or use 'gh dash' and 'gh spec' to explore."
          '';
        };

        # Minimal default package so `nix build` succeeds
        packages.default = pkgs.hello;
        defaultPackage = packages.default;
      });
}
