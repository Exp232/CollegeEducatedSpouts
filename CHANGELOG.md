# Changelog

## 1.0.1

- Fixed: with Auto Engineer Brake Fix 2.7.0 installed, the game reported a circular dependency and loaded neither mod. This mod no longer lists Auto Engineer Brake Fix as an optional dependency (that one links the other way). Auto Engineer Brake Fix still finds this mod by itself.

## 1.0.0

First release as its own mod. The swinging spouts that were part of Auto Engineer Brake Fix 2.6.x (see its changelog for
how they were built and tested), with:

- its own settings window (Ctrl+F9), `Settings.xml` and `spoutswing.txt`;
- on by default;
- a start-up self-check of the game code it patches;
- a small API that Auto Engineer Brake Fix 2.7.0 and later uses, when this mod is installed, so its water stop knows what a turning spout can reach.
