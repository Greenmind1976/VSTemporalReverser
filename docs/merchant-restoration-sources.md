# Merchant Restoration Sources

Scope: vanilla `game` domain only.

Sources checked:

- `/Applications/Vintage Story 1.22.app/assets/survival/entities/humanoid/trader-female.json`
- `/Applications/Vintage Story 1.22.app/assets/survival/entities/humanoid/trader-male.json`
- `/Applications/Vintage Story 1.22.app/assets/survival/config/tradelists`
- `/Applications/Vintage Story 1.22.app/assets/game/lang/en.json`

The trader entities use `tradePropsFile: "config/tradelists/trader-{type}"`, so the useful source of truth is the vanilla trade list JSON plus the English language file for player-facing names.

## Results

I scanned 17 vanilla trade lists and 556 sell entries. Direct merchant-sold `game:clutter` is limited: 16 sell entries, 15 unique clutter types, all from `villager-tobias`. Those are mostly Tobias lamps, books, book stands, and gadget piles.

For the restoration idea, the more useful finding is that many "working" analogues are sold as registered block IDs rather than as clutter types.

## Merchant-Bypass Targets

This is the strongest gameplay framing: the player can spend temporal resources instead of buying the normal item from a trader.

| Ruined source | Merchant-sold restore target | Player-facing result | Merchant source | Avg price | Confidence |
| --- | --- | --- | --- | ---: | --- |
| `chandelier-ruined*` | `game:chandelier-candle0` | Chandelier | `trader-luxuries` selling | 17 | Very high |
| `door-ruined-*` | `game:door-sleek-windowed-oak` or other sold sleek door | Sleek door | `trader-furniture` selling | 8-11 | High |
| `chair-ruined*` | `game:chair-plain` | Chair (Plain) | `trader-furniture` selling | 2 | High |
| `table-ruined*` | `game:table-normal` | Wooden table | `trader-furniture` selling | 2 | High |
| `bed/bed-ruined*` | `game:bed-wood-head-north` | Wooden bed | `trader-furniture` selling | 8 | High |
| `bed/bed-straw-ruined*` | `game:bed-hay-head-north` | Hay bed | `trader-survivalgoods` selling | 1 | High |
| `torchholder-ruined-*` | `game:torchholder-brass-empty-north` | Brass torch holder | `trader-furniture` selling | 12 | High |
| `crate/crate-*-ruined*` | `game:crate` with `type: wood-aged` | Aged crate | `trader-furniture` selling | 2 | Medium |
| `claylamp-ruined*` | `game:lantern-large-up` with copper/plain attributes | Large copper lantern | `trader-furniture` selling | 11 | Medium |
| ruined bookshelf/scrollrack clutter | `game:bookshelf` with larch `2row1col` attributes | Bookshelf | `trader-furniture` selling | 8 | Medium |
| `shelf/metal*-ruined*` | `game:shelf-normal-east` | Shelf | `trader-furniture` selling | 2 | Medium |

The chandelier is the cleanest example. The ruined forms are `game:clutter` types, while the restored target is the real vanilla block `game:chandelier-candle0`. Vanilla's chandelier block declares `class: "BlockChandelier"` and the source comment says it handles placing candles, which matches what you saw in creative mode.

Doors are also a good newly-added candidate. Vanilla has `door-ruined-rough*`, `door-ruined-windowed*`, `door-ruined-solid*`, `door-ruined-barred*`, and `door-ruined-sleek*`. The furniture trader sells many functional sleek windowed doors plus `game:metaldoor-sleek-windowed-iron`, so a ruined door can reasonably reverse into a merchant-sold door.

## Merchant-Backed Repair Targets

These are the best candidates where the restored item has a user-facing name and a vanilla merchant source.

| Ruined target pattern | Ruined player-facing name | Restore to | Restored player-facing name | Merchant source | Avg price | Avg stock |
| --- | --- | --- | --- | --- | ---: | ---: |
| `chair-ruined*` | Ruined chair | `game:chair-plain` | Chair (Plain) | `trader-furniture` selling | 2 | 4 |
| `table-ruined*` | Ruined table | `game:table-normal` | Wooden table | `trader-furniture` selling | 2 | 2 |
| `bed/bed-ruined*` | Ruined bed | `game:bed-wood-head-north` | Wooden bed | `trader-furniture` selling | 8 | 1 |
| `bed/bed-straw-ruined*` | Ruined straw bed | `game:bed-hay-head-north` | Hay bed | `trader-survivalgoods` selling | 1 | 1 |
| `chandelier-ruined*` | Ruined chandelier | `game:chandelier-candle0` | Chandelier | `trader-luxuries` selling | 17 | 1 |
| `claylamp-ruined*` | No specific `en.json` name found | `game:lantern-large-up` with copper/plain attributes | Large copper lantern | `trader-furniture` selling | 11 | 2 |
| `torchholder-ruined-*` | Ruined torch holder | `game:torchholder-brass-empty-north` | Brass torch holder | `trader-furniture` selling | 12 | 2 |
| shelf-like ruined clutter | Ruined metal shelf | `game:shelf-normal-east` | Shelf | `trader-furniture` selling | 2 | 6 |
| `crate/crate-*-ruined*` | Small ruined crate / Large ruined crate | `game:crate` with `type: wood-aged` | Crate | `trader-furniture` selling | 2 | 8 |
| book/scrollrack ruined clutter | Ruined scroll rack | `game:bookshelf` with larch `2row1col` attributes | Bookshelf | `trader-furniture` selling | 8 | 8 |

Furniture trader also buys `game:chandelier-candle0` at average price 10, so chandelier is both sold by luxuries and accepted by furniture.

## Direct Clutter Sold

All direct clutter sell entries came from `villager-tobias`:

| Clutter type | Player-facing name | Avg price | Avg stock |
| --- | --- | ---: | ---: |
| `tobias-lantern` | Lantern | 14 | 1 |
| `tobias-ceilinglamp-lamp` | Ceiling lamp | 32 | 1 |
| `tobias-ceilinglamp-mount` | Ceiling lamp mount | 24 | 1 |
| `tobias-ceilinglamp-shaft` | Ceiling lamp shaft | 18 | 4 |
| `book-big-closed` | Closed large book | 12 | 1 |
| `book-big-open` | Opened large book | 12 | 1 |
| `bookshelves/large-book-standing1` | Large book | 12 | 1 |
| `bookshelves/bookstand-book-closed` | Book stand | 18 | 1 |
| `pile-tobias-gadgets1` through `pile-tobias-gadgets5` | Clutter | 24 | 1 |

Some Tobias book clutter falls back to the generic player-facing name "Clutter" because `en.json` does not define a specific clutter name for every type.

## Important Distinction

These vanilla targets exist and have user-facing names, but I did not find them in merchant sell lists:

- `game:chair-aged` - Aged wooden chair
- `game:table-aged` - Aged wooden table
- `game:bed-woodaged-head-north` - Aged wooden bed
- `game:torchholder-aged-empty-north` - Empty Aged Torch holder
- `game:stove-unlit-north` - Stove (Unlit)
- `game:toolrack-north` - Tool rack
- `game:scrollrack` / scrollrack variants - scroll racks

That means we have two viable mapping modes:

1. Merchant-backed mode: ruined furniture becomes the closest thing merchants sell, such as plain chair, normal table, wooden bed, brass torch holder.
2. Archaeological/restoration mode: ruined furniture becomes an aged or shape-matched vanilla target that exists in assets, even if merchants do not sell it.

For gameplay, I would start with merchant-backed mode for version one because it gives us a clean "this item exists in survival economy" justification and avoids turning every ruin object into rare decorative clutter.
