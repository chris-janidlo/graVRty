mkdir -p Assets/Prefabs
mkdir -p Assets/ScriptableObjects/Atoms
mkdir -p Assets/Scripts

rm LICENSE
rm README.md
rm init.sh

git add .
git commit -m "Automated cleanup commit"
git push
