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

        # GitHub CLI extensions
        ghDash = pkgs.fetchFromGitHub {
          owner = "winterschon";
          repo = "gh-dash";
          rev = "main";
          sha256 = "sha256-PLACEHOLDER"; # Replace with actual hash
        };

        specKit = pkgs.fetchFromGitHub {
          owner = "github";
          repo = "spec-kit";
          rev = "main";
          sha256 = "sha256-PLACEHOLDER"; # Replace with actual hash
        };
      in {
        devShells.default = pkgs.mkShell {
          name = "dotnet-neovim-agentic-shell";

          buildInputs = [
            pkgs.dotnet-sdk_8
            pkgs.omnisharp-roslyn
            pkgs.netcoredbg
            pkgs.nodejs_20
            pkgs.neovim
            pkgs.git
            pkgs.gh
            pkgs.ripgrep
            pkgs.fd
            pkgs.vimPlugins.nvim-lspconfig
            pkgs.vimPlugins.nvim-cmp
            pkgs.vimPlugins.nvim-dap
            pkgs.vimUtils.buildVimPlugin {
              pname = "copilot.vim";
              version = "1.0";
              src = pkgs.fetchFromGitHub {
                owner = "github";
                repo = "copilot.vim";
                rev = "main";
                sha256 = "sha256-0000000000000000000000000000000000000000000000000000"; # Replace with actual hash
              };
            }
          ];

          shellHook = ''
            export XDG_CONFIG_HOME=$(mktemp -d)
            export XDG_DATA_HOME=$(mktemp -d)
            export XDG_CACHE_HOME=$(mktemp -d)

            mkdir -p $XDG_CONFIG_HOME/nvim
            echo 'require("lspconfig").omnisharp.setup{}' > $XDG_CONFIG_HOME/nvim/init.lua

            # Install GitHub CLI extensions
            gh extension install ${ghDash}
            gh extension install ${specKit}

            echo "✅ Agentic .NET dev shell ready. Run 'nvim' or use 'gh dash' and 'gh spec' to explore."
          '';
        };
      });
}
