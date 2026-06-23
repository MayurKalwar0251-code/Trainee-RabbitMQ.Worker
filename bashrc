gpush() {
  branch=$(git branch --show-current)
  git add . &&
  git commit -m "$1" &&
  git push origin "$branch"
}
 
gco() {
  git checkout "$1"
}
 
gsw() {
  git switch "$1"
}
 
gst() {
  git status
}
 
gpl() {
  git pull
}
